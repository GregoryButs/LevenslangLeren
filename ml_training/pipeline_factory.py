"""
=============================================================================
Chain-of-Thought (CoT) Rationale voor de Pipeline Volgorde:
=============================================================================
Bij het ontwerpen van een Machine Learning pipeline voor een 'no-show' voorspellingsmodel
is een logische, robuuste en modulaire volgorde van transformaties essentieel om datalekken 
(data leakage) te voorkomen en optimale modelprestaties te garanderen.

1. **Parallelle feature-specifieke verwerking (ColumnTransformer als eerste stap):**
   We splitsen onze features op basis van hun datatypes en semantische betekenis:
   - **Continue variabelen ('age', 'sessions_completed'):** Deze worden geschaald met `StandardScaler`.
     Dit zorgt ervoor dat variabelen met een groter bereik (zoals leeftijd) de gradiënt-afdaling 
     van lineaire modellen/neurale netwerken niet domineren.
   - **Categorische variabelen ('treatment_type'):** Deze worden omgezet met `OneHotEncoder`.
     We configureren dit met `handle_unknown='ignore'` en `sparse_output=False`. Dit is cruciaal 
     voor productieomgevingen: als er in de toekomst een nieuw type behandeling wordt geïntroduceerd, 
     faalt de pipeline niet, maar negeert deze simpelweg de onbekende categorie.
   - **Specifieke causale variabelen ('last_session_gap'):** De klinische hypothese is dat patiënten 
     die langer dan 14 dagen (twee weken) geen sessie hebben gehad, een significant hoger risico 
     lopen op een no-show. Dit modelleren we expliciet met een binaire drempelwaarde-indicator 
     (`GapRiskTransformer` met drempelwaarde 14). Door dit als een binaire indicator te encoderen, 
     maken we het voor lineaire classifiers (zoals logistische regressie) veel eenvoudiger om dit 
     niet-lineaire drempeleffect te leren zonder ingewikkelde polynomiale features te introduceren.

2. **Integratie van Estimator (Classifier):**
   De ColumnTransformer fungeert als de preprocessing-stap in de hoofd-`Pipeline`. Direct daarna 
   sluit de classifier (bijvoorbeeld `LogisticRegression` of een tree-based model) aan. 
   Omdat alle preprocessing-stappen (schalen, one-hot encoding, en binarisatie) binnen de scikit-learn 
   pipeline vallen, wordt de transformatie-logica automatisch toegepast tijdens zowel `.fit()` 
   (trainingsfase) als `.predict()` (productiefase). Dit sluit elke vorm van data leakage uit en 
   vereenvoudigt de deployment naar productie aanzienlijk.
=============================================================================
"""

import os
import numpy as np
import pandas as pd
from sklearn.base import BaseEstimator, TransformerMixin
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import StandardScaler, OneHotEncoder
from sklearn.linear_model import LogisticRegression

class GapRiskTransformer(BaseEstimator, TransformerMixin):
    """
    Custom Scikit-Learn Transformer die de numerieke feature 'last_session_gap'
    omzet in een binaire feature 'high_risk' (1 als gap > 14 dagen, anders 0).
    """
    def __init__(self, threshold=14):
        self.threshold = threshold
        
    def fit(self, X, y=None):
        # Scikit-learn API compatibiliteit: fit doet niets maar retourneert self
        return self
        
    def transform(self, X):
        # Zorg dat we werken met een DataFrame of Numpy array
        if isinstance(X, pd.DataFrame):
            X_input = X.values
        else:
            X_input = X
            
        # Zorg ervoor dat de input 2D is voor consistenter gedrag
        if len(X_input.shape) == 1:
            X_input = X_input.reshape(-1, 1)
            
        # Pas de binaire drempelwaarde (> 14 dagen) toe
        binary_risk = (X_input > self.threshold).astype(int)
        return binary_risk

    def get_feature_names_out(self, input_features=None):
        """
        Zorgt voor scikit-learn compatibiliteit bij het ophalen van featurenamen
        via pipeline.get_feature_names_out().
        """
        return np.array(['high_risk'], dtype=object)

class PipelineFactory:
    """
    Factory klasse voor het genereren van een robuuste scikit-learn
    pipeline voor no-show voorspellingen.
    """
    
    @staticmethod
    def create_pipeline(classifier=None):
        """
        Bouwt en retourneert de scikit-learn Pipeline met preprocessing
        en de opgegeven classifier (standaard LogisticRegression).
        """
        if classifier is None:
            classifier = LogisticRegression(random_state=42)
            
        # 1. Definieer welke kolommen naar welke transformatie gaan
        numeric_features = ['age', 'sessions_completed']
        categorical_features = ['treatment_type']
        gap_feature = ['last_session_gap']
        
        # 2. Definieer de specifieke preprocessing pipelines
        numeric_transformer = Pipeline(steps=[
            ('scaler', StandardScaler())
        ])
        
        categorical_transformer = Pipeline(steps=[
            ('onehot', OneHotEncoder(handle_unknown='ignore', sparse_output=False))
        ])
        
        gap_transformer = Pipeline(steps=[
            ('risk_binarizer', GapRiskTransformer(threshold=14))
        ])
        
        # 3. Combineer alle transformaties in een ColumnTransformer
        preprocessor = ColumnTransformer(
            transformers=[
                ('num', numeric_transformer, numeric_features),
                ('cat', categorical_transformer, categorical_features),
                ('gap', gap_transformer, gap_feature)
            ],
            remainder='drop'  # Drop eventuele identifiers zoals client_id, email, name
        )
        
        # 4. Bouw de uiteindelijke pipeline inclusief classifier
        full_pipeline = Pipeline(steps=[
            ('preprocessor', preprocessor),
            ('classifier', classifier)
        ])
        
        return full_pipeline

# Demonstratie van de Pipeline
if __name__ == "__main__":
    # Laad de gegenereerde data als deze bestaat
    csv_path = 'synthetic_patients.csv'
    if os.path.exists(csv_path):
        df = pd.read_csv(csv_path)
        
        # Splits X en y
        X = df[['age', 'sessions_completed', 'treatment_type', 'last_session_gap']]
        y = df['no_show']
        
        # Genereer de pipeline via de factory
        pipeline = PipelineFactory.create_pipeline()
        
        # Fit het model
        print("Model fitten op de synthetische data...")
        pipeline.fit(X, y)
        print("Model fit succesvol voltooid!\n")
        
        # Test met een hypothetische patiënt (last_session_gap > 14 dagen)
        test_patient = pd.DataFrame([{
            'age': 34,
            'sessions_completed': 5,
            'treatment_type': 'depressie',
            'last_session_gap': 18  # > 14 dagen, dus high_risk = 1
        }])
        
        prediction = pipeline.predict(test_patient)
        prob = pipeline.predict_proba(test_patient)[0][1]
        
        # Haal de getransformeerde features op ter verificatie
        preprocessed_data = pipeline.named_steps['preprocessor'].transform(test_patient)
        feature_names = pipeline.named_steps['preprocessor'].get_feature_names_out()
        
        print("-" * 60)
        print("Preprocessed Features voor testpatiënt:")
        for name, val in zip(feature_names, preprocessed_data[0]):
            print(f"  {name}: {val:.4f}")
        print("-" * 60)
        print(f"Test Patiënt Status:")
        print(f"  - Leeftijd: 34")
        print(f"  - Voltooide sessies: 5")
        print(f"  - Behandeling: depressie")
        print(f"  - Gap sinds vorige sessie: 18 dagen (> 14 drempel)")
        print(f"Voorspelde klasse: {'No-Show' if prediction[0] == 1 else 'Aanwezig'}")
        print(f"Berekende kans op No-Show: {prob:.2%}")
        print("-" * 60)
    else:
        print(f"Bestand '{csv_path}' niet gevonden. Run eerst 'generate_synthetic_data.py' om data te genereren.")
