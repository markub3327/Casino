using System;
using System.Text;
using System.Security.Cryptography;

namespace Casino.Server
{
    public static class TokenGenerator
    {
        public static void Generate(Items.Player player)
        {
            // Generator hashov
            using (SHA512 hashGen = new SHA512Managed())
            {
                // Primarny kodovaci kluc (format tokenu)
                string key = $"{player.Name}";
                
                // Hash
                var hash = hashGen.ComputeHash(Encoding.UTF8.GetBytes(key));

                for (int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] < 0x20) // SPACE
                        hash[i] += 0x20;
                    else if (hash[i] > 0x7e)    // ~
                        hash[i] -= 0x7e;
                }

                // Vygeneruje retazec tokenu podla kluca
                player.Token = Encoding.UTF8.GetString(hash);
            }
        }
    }
}