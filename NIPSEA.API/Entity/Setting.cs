namespace NIPSEA.API.Entity
{
    public class JWTSetting
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Domain { get; set; }
        public string SecretKey { get; set; }
        public int TokenDurationInMin { get; set; }
        public int RefreshTokenDurationInHour { get; set; }
        public string RefreshTokenEncrptKey { get; set; }
    }
}
