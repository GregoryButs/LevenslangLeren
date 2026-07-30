import sqlite3
import pandas as pd
import os

def fetch_real_world_data(db_path=None):
    """
    Verbindt met de actieve SQLite database van het CRM en haalt de historische
    afspraken en cliëntkenmerken op om correlaties te berekenen en modellen te trainen.
    """
    if db_path is None:
        # Bepaal het relatieve pad ten opzichte van dit script
        current_dir = os.path.dirname(os.path.abspath(__file__))
        db_path = os.path.abspath(os.path.join(current_dir, "../Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Afsprakenbeheer.db"))
    
    if not os.path.exists(db_path):
        raise FileNotFoundError(f"Database bestand niet gevonden op: {db_path}")
        
    print(f"Verbinding maken met database: {db_path}...")
    conn = sqlite3.connect(db_path)
    
    # SQL query om de historische trajecten en gaten tussen afspraken te berekenen
    query = """
    WITH HistoricAppointments AS (
        SELECT 
            a.Id,
            a.PatientId,
            a.Starttijd,
            a.Status,
            at.Naam AS TreatmentType,
            p.Geboortedatum,
            -- Haal de starttijd van de VORIGE afspraak van dezelfde patiënt op
            LAG(a.Starttijd) OVER (
                PARTITION BY a.PatientId 
                ORDER BY a.Starttijd
            ) AS VorigeStarttijd
        FROM Afspraken a
        JOIN Patienten p ON a.PatientId = p.Id
        JOIN AfspraakTypes at ON a.TypeId = at.Id
        WHERE a.Status IN ('Voltooid', 'Geannuleerd') -- Alleen historische afspraken
    )
    SELECT 
        Id AS AppointmentId,
        PatientId,
        Starttijd AS AppointmentDate,
        TreatmentType,
        -- Status omzetten naar binaire no_show indicator (1=No-Show, 0=Aanwezig)
        CASE WHEN Status = 'Geannuleerd' THEN 1 ELSE 0 END AS no_show,
        -- Leeftijd berekenen op het moment van de afspraak
        CAST((strftime('%Y', Starttijd) - strftime('%Y', Geboortedatum)) AS INTEGER) AS age,
        -- Verschil in dagen (gap) berekenen tussen de afspraken
        CAST((julianday(Starttijd) - julianday(VorigeStarttijd)) AS INTEGER) AS last_session_gap
    FROM HistoricAppointments;
    """
    
    df = pd.read_sql_query(query, conn)
    conn.close()
    
    # Eerste afspraak van een patiënt heeft nog geen "last_session_gap", vul met standaard wekelijks interval (7)
    df['last_session_gap'] = df['last_session_gap'].fillna(7).astype(int)
    
    return df

if __name__ == "__main__":
    try:
        df_real = fetch_real_world_data()
        print("\n=== Data Succesvol Opgehaald ===")
        print(f"Totaal aantal historische records: {len(df_real)}")
        
        # Sla de data op als real_patients.csv
        current_dir = os.path.dirname(os.path.abspath(__file__))
        output_csv = os.path.join(current_dir, "real_patients.csv")
        df_real.to_csv(output_csv, index=False)
        print(f"Data succesvol opgeslagen in: {output_csv}")
        
        print("\nEerste 5 rijen:")
        print(df_real.head().to_string(index=False))
        
        # Toon verdeling van no-shows
        no_show_counts = df_real['no_show'].value_counts()
        print("\nVerdeling aanwezigheid:")
        print(f"  Aanwezig: {no_show_counts.get(0, 0)}")
        print(f"  No-Show:  {no_show_counts.get(1, 0)}")
        
    except Exception as e:
        print(f"\nFout bij ophalen data: {e}")
