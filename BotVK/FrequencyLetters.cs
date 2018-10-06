using System;
using System.Text;

namespace BotVK
{
    class FrequencyLetters
    {
        private int[] freqRus;
        private int[] freqEng;
        private int fullCount;
        private int Length
        {
            get
            {
                return freqRus.Length + freqEng.Length;
            }
        }

        public static FrequencyLetters operator +(FrequencyLetters fl1, FrequencyLetters fl2)
        {
            FrequencyLetters sum = new FrequencyLetters();
            for (int i = 0; i < fl1.fullCount; i++)
                sum[i] = fl1[i] + fl2[i];

            sum.fullCount = fl1.fullCount + fl2.fullCount;

            return sum;
        }

        public FrequencyLetters()
        {
            freqRus = new int[32];
            freqEng = new int[26];
            fullCount = 0;
        }

        public int this[int index]
        {
            get
            {
                int i = index;
                if (index > 32)
                {
                    i = index - 32;
                    return freqEng[i];
                }

                return freqRus[i];
            }
            set
            {
                int i = index;
                if (index > 32)
                {
                    i = index - 32;
                    freqEng[i] = value;
                }

                freqRus[i] = value;
            }
        }
        public int this[char letter]
        {
            get
            {
                int code = (int)letter;

                if (code >= 97 && code <= 122)//английские буквы
                {
                    return freqEng[code - 97];
                }

                return freqRus[code - 1072];
            }
        }

        public void Add(char letter)
        {
            int code = (int)letter;

            if (code >= 1072 && code <= 1103)//русские буквы
            {
                freqRus[code - 1072]++;
                fullCount++;
            }

            if (code >= 97 && code <= 122)//английские буквы
            {
                freqEng[code - 97]++;
                fullCount++;
            }
        }

        public void Add(string Text)
        {
            string newText = Text.Replace('ё', 'е').ToLower();

            for (int i = 0; i < newText.Length; i++)
            {
                this.Add(newText[i]);
            }
        }

        public string toJSON()
        {
            StringBuilder sb = new StringBuilder();

            bool empty = true;

            sb.AppendLine("{");
            sb.AppendLine("  \"Language\": \"Русский\",");
            sb.Append("  \"Letters\": {");
            for (int i = 0; i < freqRus.Length; i++)
            {
                if (freqRus[i] > 0)
                {
                    string c = ((char)(i + 1072)).ToString();
                    if (i == 5)
                        c = "е/ё";
                    sb.Append(string.Format("\"{0}\": \"{1}\", ", c, Math.Round(((double)freqRus[i] / fullCount), 3)));
                    empty = false;
                }
            }
            if (!empty)
                sb.Remove(sb.Length - 2, 2);
            sb.AppendLine(" },");

            sb.AppendLine("  \"Language\": \"English\",");
            sb.Append("  \"Letters\": {");
            for (int i = 0; i < freqEng.Length; i++)
            {
                if (freqEng[i] > 0)
                {
                    sb.Append(string.Format("\"{0}\": \"{1}\", ", (char)(i + 97), Math.Round(((double)freqEng[i] / fullCount), 3)));
                    empty = false;
                }
            }
            if (!empty)
                sb.Remove(sb.Length - 2, 2);

            sb.AppendLine(" }");
            sb.Append("}");

            return sb.ToString();
        }
    }
}
