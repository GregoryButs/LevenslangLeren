using System.ComponentModel.DataAnnotations;


namespace CommeChesSwa.Validation
{
    public class ExpectedValue: ValidationAttribute
    {
        private bool expectedValue;

        public ExpectedValue(bool value)
        {
            this.expectedValue = value;
        }

        public override bool IsValid(object? value)
        {
            return (bool)value == expectedValue;
        }
    }
}

