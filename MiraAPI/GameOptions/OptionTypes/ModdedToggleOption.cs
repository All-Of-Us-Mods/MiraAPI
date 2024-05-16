namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedToggleOption : ModdedOption<bool>
    {
        public ModdedToggleOption(string title, bool defaultValue = false) : base(title, defaultValue)
        {
        }

        public override bool GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
        {
            return optionBehaviour.GetBool();
        }
    }
}
