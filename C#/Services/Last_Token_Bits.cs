namespace Voyagers.Services
{
    public static class Last_Token_Bits
    {
        public static string getLastTokenBits(byte[] token)
        {
            return token[7].ToString();
        }
    }
}
