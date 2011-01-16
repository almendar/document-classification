using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace document_classification
{
    class DCDbTools
    {
        private MySqlConnection conn;

        public DBRepresentation getDBRepresentation() { return null; }
        public AllCases getAllCases() { return null; }
        private void connect();
        private void disconnect();
    }
}
