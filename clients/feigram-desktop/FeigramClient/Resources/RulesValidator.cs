using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FeigramClient.Resources
{
    public class RulesValidator
    {
        private static string PatronEmail = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        private static string PatronContraseña = @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&()_+\[\]{};':""\\|,.<>/?-]).{8,64}$";

        public void AddLimitToTextBox(TextBox textBox, int limite)
        {
            textBox.TextChanged += (sender, e) =>
            {
                if (textBox.Text.Length > limite)
                {
                    textBox.Text = textBox.Text.Substring(0, limite);
                    textBox.CaretIndex = textBox.Text.Length;
                }

                string textoActual = textBox.Text.Replace("  ", " ");
                if (textoActual != textBox.Text)
                {
                    textBox.Text = textoActual;
                    textBox.CaretIndex = textBox.Text.Length;
                }

                if (textBox.Text.StartsWith(" "))
                {
                    textBox.Text = textBox.Text.TrimStart();
                    textBox.CaretIndex = textBox.Text.Length;
                }

                if (textBox.Text.Length == 1 && textBox.Text == " ")
                {
                    textBox.Text = string.Empty;
                }

                if (textBox.Text.Length > 1 &&
                    textBox.Text.Substring(textBox.Text.Length - 2) == "  ")
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            };
        }


        public void AddLimitToPasswordBox(PasswordBox passwordBox, int limite)
        {
            passwordBox.PasswordChanged += (sender, e) =>
            {

                string currentPassword = passwordBox.Password;

                if (currentPassword.Length > limite)
                {
                    currentPassword = currentPassword.Substring(0, limite);
                }

                if (currentPassword.StartsWith(" "))
                {
                    currentPassword = currentPassword.TrimStart();
                }

                string cleanPassword = currentPassword.Replace(" ", "");
                if (cleanPassword != passwordBox.Password)
                {
                    passwordBox.Password = cleanPassword;
                }

                passwordBox.Focus();
            };
        }

        public bool EmailValidator(string email)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, PatronEmail))
            {
                isValid = false;
            }
            return isValid;
        }

        public bool PasswordValidator(string contraseña)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(contraseña) || !Regex.IsMatch(contraseña, PatronContraseña))
            {
                isValid = false;
            }
            return isValid;
        }

        public void EviteDangerLettersInTextbox(TextBox textBox)
        {
            textBox.TextChanged += (sender, e) =>
            {
                string caracteresPeligrosos = @"'"";--/*()%[]{}!#$&=?¡_:+¿|,~°´¨";

                string textoFiltrado = new string(textBox.Text
                    .Where(c => !caracteresPeligrosos.Contains(c))
                    .ToArray());

                if (textoFiltrado != textBox.Text)
                {
                    int posicionCursor = textBox.CaretIndex;
                    textBox.Text = textoFiltrado;
                    textBox.CaretIndex = Math.Min(posicionCursor, textoFiltrado.Length);
                }
            };
        }

        public void EviteDangerLettersInPasswordBox(PasswordBox passwordBox)
        {
            passwordBox.PasswordChanged += (sender, e) =>
            {
                string caracteresPeligrosos = @"'"";--/*()%[]{}!#$&=?¡_:+¿|,~°´¨";

                string textoFiltrado = new string(passwordBox.Password
                    .Where(c => !caracteresPeligrosos.Contains(c))
                    .ToArray());

                if (textoFiltrado != passwordBox.Password)
                {
                    passwordBox.Password = textoFiltrado;
                }
            };
        }

    }
}
