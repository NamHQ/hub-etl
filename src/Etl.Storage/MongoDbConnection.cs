using System.Text;

namespace Etl.Storage
{
    public class MongoDbConnection
    {
        public string Service { get; set; } = "localhost";
        public int Port { get; set; } = 27017;

        public string Username { get; set; } = "admin";

        public string Password { get; set; } = "admin";

        public string DbName { get; set; }

        public string GetConnectionString()
        {
            //"mongodb://admin:admin@localhost:27017"

            var sb = new StringBuilder("mongodb://");
            if (!string.IsNullOrEmpty(Username))
                sb.Append($"{Username}:{Password}@");

            sb.Append($"{Service}:{Port}");

            return sb.ToString();
        }

        public override string ToString()
            => GetConnectionString();
    }
}
