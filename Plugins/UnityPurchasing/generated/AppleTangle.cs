#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class AppleTangle
    {
        private static byte[] data = System.Convert.FromBase64String("PGoEtjUlMjdhKRQwtjU8BLY1MASDL4mndhAmHvM7KYJ5qGpX/H+0I/0tRsFpOuFLa6/GETeOYbt5aTnFMDInNmFnBScEJTI3YTA+Jz51REQUW1IUQFxRFEBcUVoUVUREWF1XVXFKK3hfZKJ1vfBAVj8kt3WzB761K6XvKnNk3zHZak2wGd8ClmN4YdhTuzyAFMP/mBgUW0SCCzUEuIN3+yIEIDI3YTA3Jzl1RERYURRmW1tATRRVR0dBWVFHFFVXV1FEQFVaV1EBBgUABAcCbiM5BwEEBgQNBgUABDupCccffRwu/Mr6gY067Woo4v8JVlhRFEdAVVpQVUZQFEBRRllHFFVLdZyszeX+UqgQXyXkl4/QLx73K0ZVV0BdV1EUR0BVQFFZUVpARxoEMjdhKTowIjAgH+Rdc6BCPcrAX7ntAkv1s2Htk62NBnbP7OFFqkqVZjkyPR6yfLLDOTU1MTE0N7Y1NTRoWlAUV1taUF1AXVtaRxRbUhRBR1EEtjCPBLY3l5Q3NjU2NjU2BDkyPaGqTjiQc79v4CIDB//wO3n6IF3lPB8yNTExMzY1IipcQEBERw4bG0O/Lb3qzX9YwTOfFgQ23CwKzGQ957tHtVTyL289G6aGzHB8xFQMqiHBBwJuBFYFPwQ9MjdhMDInNmFnBScadJLDc3lLPGoEKzI3YSkXMCwEIhgUV1FGQF1SXVdVQFEURFtYXVdNQ0MaVUREWFEaV1tZG1VERFhRV1VQARchfyFtKYegw8KoqvtkjvVsZE4EtjVCBDoyN2EpOzU1yzAwNzY1XVJdV1VAXVtaFHVBQFxbRl1ATQUesnyywzk1NTExNARWBT8EPTI3YRR3dQS2NRYEOTI9HrJ8ssM5NTU1bZMxPUgjdGIlKkDng78XD3OX4VsUVVpQFFdRRkBdUl1XVUBdW1oURIrAR6/a5lA7/017AOyWCs1My1/8n5dFpnNnYfWbG3WHzM/XRPnSl3j0VwdDww4zGGLf7jsVOu6ORy17gRDW3+WDROs7cdUT/sVZTNnTgSMjRFhRFHdRRkBdUl1XVUBdW1oUdUEz2EkNt79nFOcM8IWLrns+X8sfyLY1NDI9HrJ8ssNXUDE1BLXGBB4yQFxbRl1ATQUiBCAyN2EwNyc5dUQSBBAyN2EwPycpdUREWFEUd1FGQAQlMjdhMD4nPnVERFhRFH1aVxoFgQ6ZwDs6NKY/hRUiGkDhCDnvViK0IB/kXXOgQj3KwF+5GnSSw3N5S1hRFH1aVxoFEgQQMjdhMD8nKXVEnOhKFgH+EeHtO+Jf4JYQFyXDlZgyBDsyN2EpJzU1yzAxBDc1NcsEKYUEbNhuMAa4XIe7KepRR8tTalGIMTQ3tjU7NAS2NT42tjU1NNClnT197EKrByBRlUOg/Rk2NzU0NZe2NSuxt7EvrQlzA8adr3S6GOCFpCbsQF1SXVdVQFEUVk0UVVpNFERVRkAbBLX3MjwfMjUxMTM2NgS1gi61hwKteBlMg9m4r+jHQ6/GQuZDBHv1RFhRFGZbW0AUd3UEKiM5BAIEAAZmUVhdVVpXURRbWhRAXF1HFFdRRgkSUxS+B17DObb76t+XG81nXm9QZJ6+4e7QyOQ9MwOEQUEV");
        private static int[] order = new int[] { 33,59,19,19,41,25,15,21,12,40,21,22,42,17,46,30,58,47,43,33,43,35,58,55,42,31,44,36,43,43,58,59,47,52,45,50,42,57,53,41,51,45,59,50,51,55,47,52,54,50,51,52,59,59,55,57,57,58,59,59,60 };
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