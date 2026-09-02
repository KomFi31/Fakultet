/*
 * Predstavlja logiku prozora za dodavanje SCADA objekata.
 * Prikazuje odgovarajuca polja u zavisnosti od izabranog tipa
 * i validira podatke pre dodavanja taga ili alarma u sistem.
 */

using DataConcentrator;
using DataConcentrator.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ScadaGUI
{
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();

            typeComboBox.SelectedIndex = 0;
            alarmConditionComboBox.SelectedIndex = 0;
        }

        #region Type Selection

        private void TypeComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (tagFields == null)
                return;

            tagFields.Visibility = Visibility.Collapsed;
            inputFields.Visibility = Visibility.Collapsed;
            analogFields.Visibility = Visibility.Collapsed;
            analogInputFields.Visibility = Visibility.Collapsed;
            outputFields.Visibility = Visibility.Collapsed;
            alarmFields.Visibility = Visibility.Collapsed;

            string selectedType = GetSelectedType();

            switch (selectedType)
            {
                case "AI":
                    tagFields.Visibility = Visibility.Visible;
                    inputFields.Visibility = Visibility.Visible;
                    analogFields.Visibility = Visibility.Visible;
                    analogInputFields.Visibility = Visibility.Visible;
                    break;

                case "AO":
                    tagFields.Visibility = Visibility.Visible;
                    analogFields.Visibility = Visibility.Visible;
                    outputFields.Visibility = Visibility.Visible;
                    break;

                case "DI":
                    tagFields.Visibility = Visibility.Visible;
                    inputFields.Visibility = Visibility.Visible;
                    break;

                case "DO":
                    tagFields.Visibility = Visibility.Visible;
                    outputFields.Visibility = Visibility.Visible;
                    break;

                case "Alarm":
                    alarmFields.Visibility = Visibility.Visible;
                    LoadAnalogInputTags();
                    break;
            }
        }

        private string GetSelectedType()
        {
            ComboBoxItem selectedItem =
                typeComboBox.SelectedItem as ComboBoxItem;

            if (selectedItem == null)
                return null;

            return selectedItem.Content.ToString();
        }

        private void LoadAnalogInputTags()
        {
            alarmTagComboBox.ItemsSource =
                DataConcentratorManager.Instance
                    .GetAllTags()
                    .OfType<AnalogInput>()
                    .Select(tag => tag.Name)
                    .ToList();

            if (alarmTagComboBox.Items.Count > 0)
                alarmTagComboBox.SelectedIndex = 0;
        }

        #endregion

        #region Add Object

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string selectedType = GetSelectedType();

            bool success = false;

            switch (selectedType)
            {
                case "AI":
                    success = AddAnalogInput();
                    break;

                case "AO":
                    success = AddAnalogOutput();
                    break;

                case "DI":
                    success = AddDigitalInput();
                    break;

                case "DO":
                    success = AddDigitalOutput();
                    break;

                case "Alarm":
                    success = AddAlarm();
                    break;
            }

            if (success)
            {
                MessageBox.Show(
                    "Object successfully added.",
                    "SCADA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
        }

        private bool AddAnalogInput()
        {
            if (!ValidateTagCommonFields())
                return false;

            if (!TryReadDouble(scanTimeTextBox, "Scan time", out double scanTime) ||
                !TryReadDouble(lowLimitTextBox, "Low limit", out double lowLimit) ||
                !TryReadDouble(highLimitTextBox, "High limit", out double highLimit) ||
                !TryReadDouble(deadbandTextBox, "Deadband", out double deadband) ||
                !TryReadDouble(hysteresisTextBox, "Hysteresis", out double hysteresis))
            {
                return false;
            }

            if (scanTime <= 0)
            {
                ShowValidationError("Scan time must be greater than zero.");
                return false;
            }

            if (lowLimit >= highLimit)
            {
                ShowValidationError(
                    "Low limit must be smaller than high limit.");

                return false;
            }

            if (deadband < 0 || hysteresis < 0)
            {
                ShowValidationError(
                    "Deadband and hysteresis cannot be negative.");

                return false;
            }

            AnalogInput tag = new AnalogInput
            {
                Name = nameTextBox.Text.Trim(),
                Description = descriptionTextBox.Text.Trim(),
                IOAddress = ioAddressTextBox.Text.Trim(),
                ScanTime = scanTime,
                OnScan = onScanCheckBox.IsChecked == true,
                LowLimit = lowLimit,
                HighLimit = highLimit,
                Units = unitsTextBox.Text.Trim(),
                Deadband = deadband,
                Hysteresis = hysteresis
            };

            if (!DataConcentratorManager.Instance.AddTag(tag))
            {
                ShowValidationError(
                    "A tag with the same name already exists.");

                return false;
            }

            return true;
        }

        private bool AddAnalogOutput()
        {
            if (!ValidateTagCommonFields())
                return false;

            if (!TryReadDouble(lowLimitTextBox, "Low limit", out double lowLimit) ||
                !TryReadDouble(highLimitTextBox, "High limit", out double highLimit) ||
                !TryReadDouble(initialValueTextBox, "Initial value", out double initialValue))
            {
                return false;
            }

            if (lowLimit >= highLimit)
            {
                ShowValidationError(
                    "Low limit must be smaller than high limit.");

                return false;
            }

            if (initialValue < lowLimit || initialValue > highLimit)
            {
                ShowValidationError(
                    "Initial value must be inside the defined limits.");

                return false;
            }

            AnalogOutput tag = new AnalogOutput
            {
                Name = nameTextBox.Text.Trim(),
                Description = descriptionTextBox.Text.Trim(),
                IOAddress = ioAddressTextBox.Text.Trim(),
                LowLimit = lowLimit,
                HighLimit = highLimit,
                Units = unitsTextBox.Text.Trim(),
                InitialValue = initialValue
            };

            if (!DataConcentratorManager.Instance.AddTag(tag))
            {
                ShowValidationError(
                    "A tag with the same name already exists.");

                return false;
            }

            DataConcentratorManager.Instance
                .WriteAnalogOutput(tag.Name, initialValue);

            return true;
        }

        private bool AddDigitalInput()
        {
            if (!ValidateTagCommonFields())
                return false;

            if (!TryReadDouble(
                scanTimeTextBox,
                "Scan time",
                out double scanTime))
            {
                return false;
            }

            if (scanTime <= 0)
            {
                ShowValidationError(
                    "Scan time must be greater than zero.");

                return false;
            }

            DigitalInput tag = new DigitalInput
            {
                Name = nameTextBox.Text.Trim(),
                Description = descriptionTextBox.Text.Trim(),
                IOAddress = ioAddressTextBox.Text.Trim(),
                ScanTime = scanTime,
                OnScan = onScanCheckBox.IsChecked == true
            };

            if (!DataConcentratorManager.Instance.AddTag(tag))
            {
                ShowValidationError(
                    "A tag with the same name already exists.");

                return false;
            }

            return true;
        }

        private bool AddDigitalOutput()
        {
            if (!ValidateTagCommonFields())
                return false;

            if (!TryReadDigitalValue(
                initialValueTextBox.Text,
                out bool initialValue))
            {
                ShowValidationError(
                    "Digital initial value must be 0, 1, true or false.");

                return false;
            }

            DigitalOutput tag = new DigitalOutput
            {
                Name = nameTextBox.Text.Trim(),
                Description = descriptionTextBox.Text.Trim(),
                IOAddress = ioAddressTextBox.Text.Trim(),
                InitialValue = initialValue
            };

            if (!DataConcentratorManager.Instance.AddTag(tag))
            {
                ShowValidationError(
                    "A tag with the same name already exists.");

                return false;
            }

            DataConcentratorManager.Instance
                .WriteDigitalOutput(tag.Name, initialValue);

            return true;
        }

        private bool AddAlarm()
        {
            if (alarmTagComboBox.SelectedItem == null)
            {
                ShowValidationError(
                    "An analog input tag must be selected.");

                return false;
            }

            if (!TryReadDouble(
                alarmLimitTextBox,
                "Alarm limit",
                out double limit))
            {
                return false;
            }

            ComboBoxItem conditionItem =
                alarmConditionComboBox.SelectedItem as ComboBoxItem;

            if (conditionItem == null)
            {
                ShowValidationError(
                    "Alarm condition must be selected.");

                return false;
            }

            if (string.IsNullOrWhiteSpace(alarmMessageTextBox.Text))
            {
                ShowValidationError(
                    "Alarm message cannot be empty.");

                return false;
            }

            AlarmCondition condition =
                conditionItem.Content.ToString() == "Above"
                ? AlarmCondition.Above
                : AlarmCondition.Below;

            Alarm alarm = new Alarm
            {
                TagName = alarmTagComboBox.SelectedItem.ToString(),
                Limit = limit,
                Condition = condition,
                Message = alarmMessageTextBox.Text.Trim()
            };

            if (!DataConcentratorManager.Instance.AddAlarm(alarm))
            {
                ShowValidationError(
                    "Alarm could not be added.");

                return false;
            }

            return true;
        }

        #endregion

        #region Validation

        private bool ValidateTagCommonFields()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                ShowValidationError(
                    "Tag name cannot be empty.");

                return false;
            }

            if (string.IsNullOrWhiteSpace(ioAddressTextBox.Text))
            {
                ShowValidationError(
                    "I/O address cannot be empty.");

                return false;
            }

            return true;
        }

        private bool TryReadDouble(
            TextBox textBox,
            string fieldName,
            out double value)
        {
            if (!double.TryParse(textBox.Text, out value))
            {
                ShowValidationError(
                    fieldName + " must be a valid number.");

                return false;
            }

            return true;
        }

        private bool TryReadDigitalValue(
            string text,
            out bool value)
        {
            string input = text.Trim().ToLower();

            if (input == "1" || input == "true")
            {
                value = true;
                return true;
            }

            if (input == "0" || input == "false")
            {
                value = false;
                return true;
            }

            value = false;
            return false;
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(
                message,
                "Invalid input",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        #endregion

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}