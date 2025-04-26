using HIUServices.Entities.Requests.Data;
using System.Data;
using System.Security.Cryptography;

namespace HIUServices.Helpers
{
    public static class Utilities
    {
        //public static List<Dictionary<string, object>> DataTableToObject(DataTable dataTable)
        //{
        //    var objects = new List<Dictionary<string, object>>();

        //    foreach (DataRow row in dataTable.Rows)
        //    {
        //        var dict = new Dictionary<string, object>();
        //        foreach (DataColumn col in dataTable.Columns)
        //        {
        //            dict[col.ColumnName] = row[col];
        //        }
        //        objects.Add(dict);
        //    }

        //    return objects;
        //}

        public static List<Dictionary<string, object>> DataTableToObject(DataTable dataTable)
        {
            var objects = new List<Dictionary<string, object>>();

            // Handle null DataTable
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return objects;
            }

            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    // Convert DBNull to null
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }
                objects.Add(dict);
            }

            return objects;
        }

        //public static DateTime? ConvertToLocalTime(DateTime? dateTime)
        //{
        //    if (dateTime == null) return null;

        //    // Convert to local time if it's in UTC
        //    if (dateTime.Value.Kind == DateTimeKind.Utc)
        //    {
        //        return TimeZoneInfo.ConvertTimeFromUtc(dateTime.Value, TimeZoneInfo.Local);
        //    }

        //    // Ensure the DateTime is in local time or unspecified time zone
        //    return dateTime;
        //}

        public static DateTime? ConvertToLocalTime(DateTime? dateTime)
        {
            if (dateTime == null) return null;

            // Convert to local time without changing the actual time
            if (dateTime.Value.Kind == DateTimeKind.Utc)
            {
                // Adjust to local time zone but maintain the same date and time
                return DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Local);
            }

            // Ensure the DateTime is treated as local or unspecified time zone
            return dateTime;
        }

        public static EDCH GenerateECDHKey()
        {

            EDCH edchObj = new EDCH();

            // Generate ECDH key pair
            using (var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
            {
                // Export the public key in X.509 SubjectPublicKeyInfo format
                byte[] publicKey = ecdh.PublicKey.ExportSubjectPublicKeyInfo();
                string publicKeyBase64 = Convert.ToBase64String(publicKey);

                // Export the private key (optional, for demonstration only; store securely)
                byte[] privateKey = ecdh.ExportECPrivateKey();
                string privateKeyBase64 = Convert.ToBase64String(privateKey);

                edchObj.public_key = publicKeyBase64;
                edchObj.private_key = privateKeyBase64;
            }

            // Generate a 32-byte random nonce
            byte[] nonce = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonce);
            }
            string nonceBase64 = Convert.ToBase64String(nonce);
            edchObj.nonce = nonceBase64;

            return edchObj;
        }
    }

}
