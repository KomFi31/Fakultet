/*
 * Predstavlja prozor za upis nove vrednosti izlaznog taga.
 * Validira korisnicki unos i prosledjuje vrednost Data Concentrator-u
 * za upis u analogni ili digitalni izlaz PLC simulatora.
 */

using DataConcentrator;
using DataConcentrator.Model;
using System.Windows;

namespace ScadaGUI
{
    public partial class WriteValueWindow : Window
    {
        private readonly Tag tag;

        public WriteValueWindow(Tag selectedTag)
        {
            InitializeComponent();

            tag = selectedTag;
            tagNameTextBlock.Text = selectedTag.Name;
        }

        private void Write_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (tag is AnalogOutput)
            {
                if (!double.TryParse(
                    valueTextBox.Text,
                    out double value))
                {
                    ShowError(
                        "Analog value must be a valid number.");

                    return;
                }

                bool success =
                    DataConcentratorManager.Instance
                        .WriteAnalogOutput(tag.Name, value);

                if (!success)
                {
                    ShowError(
                        "Value is outside the allowed limits.");

                    return;
                }
            }
            else if (tag is DigitalOutput)
            {
                if (!TryReadDigitalValue(
                    valueTextBox.Text,
                    out bool value))
                {
                    ShowError(
                        "Digital value must be 0, 1, true or false.");

                    return;
                }

                bool success =
                    DataConcentratorManager.Instance
                        .WriteDigitalOutput(tag.Name, value);

                if (!success)
                {
                    ShowError(
                        "Digital value could not be written.");

                    return;
                }
            }
            else
            {
                ShowError(
                    "Only output tags can be written.");

                return;
            }

            DialogResult = true;
            Close();
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

        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Invalid value",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}