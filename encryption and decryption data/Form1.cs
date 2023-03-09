using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace encryption_and_decryption_data
{
    public partial class app : Form
    {
        public app()
        {
            InitializeComponent();
            textBox4.Hide();
            label4.Hide();
            textBox5.Hide();
            label5.Hide();
        }


        #region 1_Caesar_encrypt
        public void Caesar_encrypt()
        {
            char[] plain_text = textBox1.Text.ToCharArray();
            char[] cipher = new char[9999];
            int key;
            if (textBox3.Text == "")
                key = 0;
            else
                key = Int32.Parse(textBox3.Text);

            int i;
            for (i = 0; i < plain_text.Length; i++)
            {
                char old_Char = plain_text[i];
                if (old_Char == ' ')
                {
                    cipher[i] = ' ';
                }
                else
                {
                    cipher[i] = (char)(old_Char + key);
                    if (char.IsUpper(old_Char) && cipher[i] > 'Z')
                        cipher[i] = (char)(cipher[i] - 26);
                    if (char.IsLetter(old_Char) && cipher[i] > 'z')
                        cipher[i] = (char)(cipher[i] - 26);
                }

                textBox2.Text = new string(cipher);
            }
        }
        public void Decrypt_Caesar()
        {
            char[] plain_text = textBox2.Text.ToCharArray();
            char[] cipher = new char[9999];
            int key;
            if (textBox3.Text == "")
                key = 0;
            else
                key = Int32.Parse(textBox3.Text);
            int i;
            for (i = 0; i < plain_text.Length; i++)
            {
                char old_Char = plain_text[i];
                if (old_Char == ' ')
                {
                    cipher[i] = ' ';
                }
                else
                {
                    cipher[i] = (char)(old_Char - key);
                    if (char.IsUpper(old_Char) && cipher[i] > 'Z')
                        cipher[i] = (char)(cipher[i] - 26);
                    if (char.IsLetter(old_Char) && cipher[i] > 'z')
                        cipher[i] = (char)(cipher[i] - 26);
                }
                textBox2.Text = new string(cipher);
            }
        }
#endregion


        #region 2_PLAYFAIR_cipher
        private static int Mod(int a, int b)
        {
            return (a % b + b) % b;
        }

        private static List<int> FindAllOccurrences(string str, char value)
        {
            List<int> indexes = new List<int>();

            int index = 0;
            while ((index = str.IndexOf(value, index)) != -1)
                indexes.Add(index++);

            return indexes;
        }

        private static string RemoveAllDuplicates(string str, List<int> indexes)
        {
            string retVal = str;

            for (int i = indexes.Count - 1; i >= 1; i--)
                retVal = retVal.Remove(indexes[i], 1);

            return retVal;
        }

        private static char[,] GenerateKeySquare(string key)
        {
            char[,] keySquare = new char[5, 5];
            string defaultKeySquare = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
            string tempKey = string.IsNullOrEmpty(key) ? "CIPHER" : key.ToUpper();

            tempKey = tempKey.Replace("J", "");
            tempKey += defaultKeySquare;

            for (int i = 0; i < 25; ++i)
            {
                List<int> indexes = FindAllOccurrences(tempKey, defaultKeySquare[i]);
                tempKey = RemoveAllDuplicates(tempKey, indexes);
            }

            tempKey = tempKey.Substring(0, 25);

            for (int i = 0; i < 25; ++i)
                keySquare[(i / 5), (i % 5)] = tempKey[i];

            return keySquare;
        }

        private static void GetPosition(ref char[,] keySquare, char ch, ref int row, ref int col)
        {
            if (ch == 'J')
                GetPosition(ref keySquare, 'I', ref row, ref col);

            for (int i = 0; i < 5; ++i)
                for (int j = 0; j < 5; ++j)
                    if (keySquare[i, j] == ch)
                    {
                        row = i;
                        col = j;
                    }
        }

        private static char[] SameRow(ref char[,] keySquare, int row, int col1, int col2, int encipher)
        {
            return new char[] { keySquare[row, Mod((col1 + encipher), 5)], keySquare[row, Mod((col2 + encipher), 5)] };
        }

        private static char[] SameColumn(ref char[,] keySquare, int col, int row1, int row2, int encipher)
        {
            return new char[] { keySquare[Mod((row1 + encipher), 5), col], keySquare[Mod((row2 + encipher), 5), col] };
        }

        private static char[] SameRowColumn(ref char[,] keySquare, int row, int col, int encipher)
        {
            return new char[] { keySquare[Mod((row + encipher), 5), Mod((col + encipher), 5)], keySquare[Mod((row + encipher), 5), Mod((col + encipher), 5)] };
        }

        private static char[] DifferentRowColumn(ref char[,] keySquare, int row1, int col1, int row2, int col2)
        {
            return new char[] { keySquare[row1, col2], keySquare[row2, col1] };
        }

        private static string RemoveOtherChars(string input)
        {
            string output = input;

            for (int i = 0; i < output.Length; ++i)
                if (!char.IsLetter(output[i]))
                    output = output.Remove(i, 1);

            return output;
        }

        private static string AdjustOutput(string input, string output)
        {
            StringBuilder retVal = new StringBuilder(output);

            for (int i = 0; i < input.Length; ++i)
            {
                if (!char.IsLetter(input[i]))
                    retVal = retVal.Insert(i, input[i].ToString());

                if (char.IsLower(input[i]))
                    retVal[i] = char.ToLower(retVal[i]);
            }

            return retVal.ToString();
        }

        private static string Cipher(string input, string key, bool encipher)
        {
            string retVal = string.Empty;
            char[,] keySquare = GenerateKeySquare(key);
            string tempInput = RemoveOtherChars(input);
            int e = encipher ? 1 : -1;

            if ((tempInput.Length % 2) != 0)
                tempInput += "X";

            for (int i = 0; i < tempInput.Length; i += 2)
            {
                int row1 = 0;
                int col1 = 0;
                int row2 = 0;
                int col2 = 0;

                GetPosition(ref keySquare, char.ToUpper(tempInput[i]), ref row1, ref col1);
                GetPosition(ref keySquare, char.ToUpper(tempInput[i + 1]), ref row2, ref col2);

                if (row1 == row2 && col1 == col2)
                {
                    retVal += new string(SameRowColumn(ref keySquare, row1, col1, e));
                }
                else if (row1 == row2)
                {
                    retVal += new string(SameRow(ref keySquare, row1, col1, col2, e));
                }
                else if (col1 == col2)
                {
                    retVal += new string(SameColumn(ref keySquare, col1, row1, row2, e));
                }
                else
                {
                    retVal += new string(DifferentRowColumn(ref keySquare, row1, col1, row2, col2));
                }
            }

            retVal = AdjustOutput(input, retVal);

            return retVal;
        }

        public static string Encipher_PLAYFAIR(string input, string key)
        {
            return Cipher(input, key, true);
        }

        public static string Decipher_PLAYFAIR(string input, string key)
        {
            return Cipher(input, key, false);
        }
        #endregion


        #region 3_HILL_Cipher
        public void encrypt_HILL_Cipher()
        {
            int[,] a = { { 6, 24, 1 }, { 13, 16, 10 }, { 20, 17, 15 } };
            int[,] b = { { 8, 5, 10 }, { 21, 8, 21 }, { 21, 12, 8 } };
            int i, j, t = 0;
            int[] c = new int[20];
            int[] d = new int[20];
            char[] msg = new char[20];
            msg = textBox1.Text.ToCharArray();
            for (i = 0; i < msg.Length; i++)
            {
                c[i] = msg[i] - 65;
            }

            for (i = 0; i < 3; i++)
            {
                t = 0;
                for (j = 0; j < 3; j++)
                {
                    t = t + (a[i, j] * c[j]);
                }
                d[i] = t % 26;
            }
            char new_char;
            for (i = 0; i < 3; i++)
            {
                new_char = (char)(d[i] + 65);
                textBox2.Text += new_char.ToString();
            }

        }
        public void decrypt_HILL_Cipher()
        {
            int[,] a = { { 6, 24, 1 }, { 13, 16, 10 }, { 20, 17, 15 } };
            int[,] b = { { 8, 5, 10 }, { 21, 8, 21 }, { 21, 12, 8 } };
            int i, j, t = 0;
            int[] c = new int[20];
            int[] d = new int[20];
            char[] msg = new char[20];
            msg = textBox2.Text.ToCharArray();
            for (i = 0; i < msg.Length; i++)
            {
                d[i] = msg[i] + 65;
            }

            for (i = 0; i < 3; i++)
            {
                t = 0;
                for (j = 0; j < 3; j++)
                {
                    t = t + (b[i, j] * d[j]);
                }
                c[i] = t % 26;
            }
            for (i = 0; i < 3; i++)
            {
                textBox2.Text += (char)(c[i] + 65);
            }

        }
        #endregion


        #region 4_Vigenere_cipher
         public void encrypt_Vigenere_cipher()
        {
            int i, j;
            char[] input = new char[3];

            char[] k = new char[5];
            int index = 0;
            foreach (char ch in textBox1.Text)
            {
                input[index] = ch;
                index++;
            }

            index = 0;
            foreach (char ch in textBox3.Text)
            {
                k[index] = ch;
                index++;
            }

            for (i = 0, j = 0; i < input.Length; i++, j++)
            {
                if (j >= k.Length)
                    j = 0;
                int number = (65 + (((Char.ToUpper(input[i]) - 65) + (Char.ToUpper(k[j]) - 65)) % 26));
                char x = (char)number;
                input[i] = x;
            }

            string cipher_text = new string(input);
            textBox2.Text = cipher_text;

        }
        public void decrypt_Vigenere_cipher()
        {
            int i, j, number;
            char[] input = new char[50];

            char[] k = new char[10];
            int index = 0;
            foreach (char ch in textBox1.Text)
            {
                input[index] = ch;
                index++;
            }

            index = 0;
            foreach (char ch in textBox3.Text)
            {
                k[index] = ch;
                index++;
            }

            for (i = 0, j = 0; i < textBox1.Text.Length; i++, j++)
            {
                if (j >= k.Length)
                    j = 0;
                number = (Char.ToUpper(input[i]) - 64) - (Char.ToUpper(k[j]) - 64);
                if (number < 0)
                    number = number * -1;
                number = 65 + (number % 26);
                char x = (char)number;
                input[i] = x;
            }

            string cipher_text = new string(input);
            textBox2.Text = cipher_text;
        }
        #endregion


        #region 5_rail_fence
        public void encrypt_rail_fence()
        {
            int i, j, l;
            char[] a = new char[20];
            char[] c = new char[20];
            char[] d = new char[20];

            int index = 0;
            foreach (char ch in textBox1.Text)
            {
                a[index] = ch;
                index++;
            }

            l = a.Length;
            j = 0;
            int x = 0;
            int y = 0;
            for (i = 0; i < l; i++)
            {
                if (i % 2 == 0)
                {
                    c[x] = a[i];
                    x++;
                }
                if (i % 2 == 1)
                {
                    d[y] = a[i];
                    y++;
                }
            }

            string ah = new string(c);
            string ah1 = new string(d);

            textBox2.Text = ah;
            textBox2.Text += ah1;

        }

        public void decrypt_rail_fence()
        {
            int i, k, l;
            char[] a = new char[20];
            char[] c = new char[20];
            char[] d = new char[20];

            int index = 0;
            foreach (char ch in textBox1.Text)
            {
                a[index] = ch;
                index++;
            }
            l = textBox1.Text.Length;

            if (l % 2 == 0)
                k = l / 2;
            else
                k = (l / 2) + 1;

            int x = 0;
            int y = 0;
            for (i = 0; i < l; i++)
            {
                if (i < k)
                {
                    c[x] = a[i];
                    x++;
                }
                if (i >= k)
                {
                    d[y] = a[i];
                    y++;

                }
            }

            for (i = 0; i < l; i++)
            {
                textBox2.Text += c[i].ToString() + d[i].ToString();
            }
        }

        #endregion


        #region 6_RSA
        int p, q, n, t, flag, i;
        int[] e = new int[100];
        int[] d = new int[100];
        int[] temp = new int[100];
        int[] m = new int[100];
        int[] en = new int[101];
        char[] msg = new char[100];

        double j;

        /*************************** RSA************************/
        int prime(int pr)
        {
            int i;
            j = Math.Sqrt(pr);
            for (i = 2; i <= j; i++)
            {
                if (pr % i == 0)
                    return 0;
            }
            return 1;
        }

        void ce()
        {
            int k;
            k = 0;
            for (i = 2; i < t; i++)
            {
                if (t % i == 0)
                    continue;
                flag = prime(i);
                if (flag == 1 && i != p && i != q)
                {
                    e[k] = i;
                    flag = cd(e[k]);
                    if (flag > 0)
                    {
                        d[k] = flag;
                        k++;
                    }
                    if (k == 99)
                        break;
                }
            }
        }
        int cd(int x)
        {
            int k = 1;
            while (true)
            {
                k = k + t;
                if (k % x == 0)
                    return (k / x);
            }
        }

        void encrypt()
        {
            int pt, ct, key = e[0], k, len;
            i = 0;
            len = msg.Length;
            while (i != len)
            {
                pt = m[i];
                pt = pt - 96;
                k = 1;
                for (j = 0; j < key; j++)
                {
                    k = k * pt;
                    k = k % n;
                }
                temp[i] = k;
                ct = k + 96;
                en[i] = ct;
                i++;
            }
            en[i] = -1;
            int length = textBox1.Text.Length;

            char[] encryption = new char[101];
            for (int q = 0; q < length; q++)
            {
                encryption[q] = (char)en[q];
            }



            string zx = new string(encryption);

            textBox2.Text = zx;

        }


        #endregion


        #region 7_DHKE
        /************************* Diffie Hellman Key Exchange***************************/

        int power(int a, int b, int mod)
        {
            int t;
            if (b == 1)
                return a;
            t = power(a, b / 2, mod);
            if (b % 2 == 0)
                return (t * t) % mod;
            else
                return (((t * t) % mod) * a) % mod;
        }
        int calculateKey(int a, int x, int n)
        {
            return power(a, x, n);
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {


            if (radioB_CAESAR.Checked)
            {
                Caesar_encrypt();
            }
            else if (radioB_PLAYFAIR.Checked)
            {
                textBox2.Text = Encipher_PLAYFAIR(textBox1.Text, textBox3.Text);
            }
            else if (radioB_HILL_Cipher.Checked)
            {
                encrypt_HILL_Cipher();
            }
            else if (radioB_Vigenere.Checked)
            {
                encrypt_Vigenere_cipher();
            }
            else if (radioButton5.Checked)
            {
                ///
                encrypt_rail_fence();

            }
    
            else if (radioButton7.Checked)
            {
                
                p = int.Parse(textBox4.Text);
                flag = prime(p);
                if (flag == 0)
                {
                    MessageBox.Show("WRONG INPUT");
                    Application.Exit();
                }
                q = int.Parse(textBox5.Text);
                flag = prime(q);
                if (flag == 0 || p == q)
                {
                    MessageBox.Show("WRONG INPUT");
                    Application.Exit();
                }
                int index = 0;
                foreach (char ch in textBox1.Text)
                {
                    m[index] = ch;
                    index++;
                }
                n = p * q;
                t = (p - 1) * (q - 1);
                ce();
                encrypt();

            }
            else if (radioButton8.Checked)
            {
                int n, g, x, a, y, b;

                n = int.Parse(textBox1.Text);
                g = int.Parse(textBox3.Text);
                x = int.Parse(textBox4.Text);
                y = int.Parse(textBox5.Text);
                a = power(g, x, n);
                b = power(g, y, n);
                MessageBox.Show(power(b, x, n).ToString());
                MessageBox.Show(power(a, y, n).ToString());

            }
            else
                MessageBox.Show("please check radio botton");

            
        }

       

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton7.Checked)
            {
                textBox4.Show();
                label4.Show();
                textBox5.Show();
                label5.Show();
                textBox3.Hide();
                label3.Hide();
                button2.Hide();
                label6.Text = "Caesar_Cipher";
                label7.Hide();
            }
        }

        private void radioB_CAESAR_CheckedChanged(object sender, EventArgs e)
        {
            if (radioB_CAESAR.Checked == true) { 
                    label3.Text = "Shift_no";
                    label6.Text = "Caesar_Cipher";
                    label7.Text = "ex: shift_no = 3 ";
                 
                  }
        }

        private void radioB_PLAYFAIR_CheckedChanged(object sender, EventArgs e)
        {
            if (radioB_PLAYFAIR.Checked)
            {
                label3.Text = "Kay (k)";
                label6.Text = "Playfair Cipher";
                label7.Text = "ex: k = aasdf";
            }
        }

        private void radioB_HILL_Cipher_CheckedChanged(object sender, EventArgs e)
        {
            if (radioB_HILL_Cipher.Checked)
            {
                label3.Text = "Kay (k)";
                label6.Text = "Hill Cipher";
                label7.Text = "ex:k = GYBNQKURP";
            }
        }

        private void radioB_Vigenere_CheckedChanged(object sender, EventArgs e)
        {
            if (radioB_Vigenere.Checked)
            {
                label1.Text = "Text3char";
                label3.Text = "Kay5char";
                label6.Text = "Vigenere Cipher";
                label7.Text = "5_char>ex:k = hello";
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                label1.Text = "Text";
                label3.Hide();
                label6.Text = "Rail Fence Cipher";
                label7.Hide();
                textBox3.Hide();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       

      
        private void button2_Click(object sender, EventArgs e)
{



if (radioB_CAESAR.Checked)
{
                Decrypt_Caesar();
}
else if (radioB_PLAYFAIR.Checked)
{
    textBox1.Text = Decipher_PLAYFAIR(textBox2.Text, textBox3.Text);
}
else if (radioB_HILL_Cipher.Checked)
{
                decrypt_HILL_Cipher();
    

}
else if (radioB_Vigenere.Checked)
{
                decrypt_Vigenere_cipher();
}
else if (radioButton5.Checked)
{
                decrypt_rail_fence();

}
 
else
    MessageBox.Show("please check radio botton");



}

         
    }
}
