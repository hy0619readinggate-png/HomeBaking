// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("GYpW/FxoiqqdG6cESR8Y7MjvxGBit6sJsReEJNhjkaGl8MUKuY2368tV8Mmo74jdJq3PEvde2/Us+cIbxhPDdtSu+1DgkVTrv88EqwVz5hALpHa883HdOGWwLlHZVboxAYtfQFB5t2z6cCORV/Al4VbYTM1NjD+f5nLVw3JMDYp2l4FmsO4xfFt4BhvUB5rcCNkbSarV1J/K28N0ysD37PPScaC4wgXSf1wauHNmKl1+dXyFWdBLI6gcjkJ4qrXKjwHNP3lnKS9S4GNAUm9ka0jkKuSVb2NjY2diYYChsyYwAZj9JXPO/aj4ad6dNw3M4GNtYlLgY2hg4GNjYt2M1LURgdLuG01qx1TJuWFbKEi6ZyDHeVVqvQD398NtF5BB8WBhY2Jj");
        private static int[] order = new int[] { 9,5,13,7,11,10,6,7,8,12,12,11,12,13,14 };
        private static int key = 98;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
