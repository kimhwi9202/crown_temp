#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("yCknXTaMJaJPRIdaTkzCj5rCX6VmG+g0oSuy5kORqvC/yWfaySLHGJg9Is3WsTLrYr3v+sc3AiXNlycNSWVNOPnlpvZms2FKauhr8H2vPHampDXzKP2N/5l8B/+e/MOO5B5S5IOdBsQKtBz4PgJc/XIjJBsGmGMMBLY1FgQ5Mj0esnyywzk1NTUxNDdAqNSF7uO23A7BC5UaNi1BnTgiQXyUYlNRlXpOZamGn6LjW9zLNjMUZf9z1gDybAjmzz8qN4kmLdOL6/8+SLRI4KpXSjNBp4gYv3PbvmZcmvoaP8rKcoadAwSN+BE5OdYSDvThtjU7NAS2NT42tjU1NKSvp2RakZFZTF9cg0WFY+LB9k8+dqTL8Mgua2F8KboIvP//jzY3NTQ1");
        private static int[] order = new int[] { 7,13,5,6,10,8,7,8,8,12,10,11,13,13,14 };
        private static int key = 52;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
