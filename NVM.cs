using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCT.Classes
{
    public class NVM
    {
        public string EMPTYSTRING = "";
        public CustomEvents _ce = new CustomEvents();

        public CustomEvents ce { get { return _ce; } set { _ce = value; } }

        public string UserID = "";
        public string UserName = "";
        public string Password = "";
        public string ToAddress = "";

        public int VERT_MARGIN = 10;
        public int HORIZ_MARGIN = 10;

        public int HORIZ_MARGIN_SMALL = 5;
        public int VERT_MARGIN_SMALL = 5;

        public int STANDARD_X_ZERO = 0;
        public int STANDARD_Y_ZERO = 0;

        public int STANDARD_LEFT_MARGIN_ZERO = 0;
        public int STANDARD_LEFT_INDENT = 4;

        public int STANDARD_CI_LISTBOX_WIDTH = 400;
        public int STANDARD_CI_DESCRIPTION_WIDTH = 400;
        public int STANDARD_CI_TEXTBOX_WIDTH = 400;
        public int STANDARD_TAG_LISTBOX_WIDTH = 250;
              
        //CONSTRUCTOR
        public NVM()
        {
            _TIVManager = new TagItemValueManager();
        }

        public TagItemValueManager TIVManager
        {
            get { return _TIVManager; }
        }

        public TagItemValueManager _TIVManager;

        public int UserIDAsInteger
        {
            get { return Convert.ToInt32(UserID); }
        }

        public bool SelectedIndexIsValid(int i)
        {
            return (i > -1);
        }

        public string DoubleUpQuotes(string temp)
        {
            string singlequotes = EMPTYSTRING;
            string doublequotes = EMPTYSTRING;
            singlequotes = temp.Replace("'", "''");
            doublequotes = singlequotes.Replace("\"", "\"\"");
            return doublequotes;
        }

        public void ToggleMouseBorder(ref System.Windows.Forms.Button btn)
        {
            if (btn.FlatAppearance.BorderSize > 0)
            {
                btn.FlatAppearance.BorderSize = 0;
            }
            else
            {
                btn.FlatAppearance.BorderSize = 1;
            }
        }

        // Convert an object to a byte array
        public byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public string CreateSQLINClause(List<string> soc)
        {
            string tempStr = EMPTYSTRING;

            if (soc.Count > 0)
            {
                foreach (var temp in soc)
                {
                    tempStr += "'" + temp.ToString() + "',";
                }

                int lastcomma = tempStr.LastIndexOf(",");

                tempStr = tempStr.Substring(0, lastcomma);

                tempStr = " where tag in (" + tempStr + ")";
            }

            return tempStr;
        }

        public void ChangeButtonImageResource(System.Windows.Forms.Button tempButton, System.Drawing.Bitmap bm)
        {
            tempButton.Image = bm;
            tempButton.BackColor = System.Drawing.Color.Transparent;
            tempButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            tempButton.FlatAppearance.BorderSize = 0;
            tempButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            tempButton.ForeColor = System.Drawing.Color.Transparent;
            tempButton.Margin = new System.Windows.Forms.Padding(0);
            tempButton.UseVisualStyleBackColor = true;
        }

        public void Email(string htmlString,
            string fromAddress,
            string toAddress,
            string Subject,
            string Host,
            int Port,
            string UserName,
            string Password)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(fromAddress);
                message.To.Add(new MailAddress(toAddress));
                message.Subject = Subject;
                message.IsBodyHtml = false;
                message.Body = htmlString;
                smtp.Port = Port;
                smtp.Host = Host; //for gmail host  
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(UserName, Password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public long IPAddressTooLong(System.Net.IPAddress address)
        {
            byte[] byteIP = address.GetAddressBytes();

            long ip = (long)byteIP[3] << 24;
            ip += (long)byteIP[2] << 16;
            ip += (long)byteIP[1] << 8;
            ip += (long)byteIP[0];
            return ip;
        }

        public static string DB_FILENAME = @"DataSource=" + Application.StartupPath + "\\CBDB.db;";
        
        public SQLiteConnection m_dbConnection = new SQLiteConnection(DB_FILENAME);
        
                    
        public bool ItemValueIsAWebURL(string tempItem)
        {
            string expr = @"^(http|http(s)?://)?([\w-]+\.)+[\w-]+[.com|.in|.org]+(\[\?%&=]*)?";
            MatchCollection mc = Regex.Matches(tempItem, expr);
            return (mc.Count > 0) ? true : false; 
        }

        public string CreateTagSQLINClause(List<string> soc)
        {
            string tempStr = EMPTYSTRING;

            if (soc.Count > 0)
            {
                foreach (string temp in soc)
                {
                    tempStr += "'" + temp + "',";
                }

                int lastcomma = tempStr.LastIndexOf(",");

                tempStr = tempStr.Substring(0, lastcomma);

                tempStr = " where tag in (" + tempStr + ")";
            }

            return tempStr;
        }       

        //this method call fills the in-memory List< > for all TAGS, ITEMS and VALUES
        //Should only be called once
        //TO DO:  Move to the constructor, depends on UserID being filled first
        public void FillTagItemValuesFromDatabase()
        {
            bool userIDIsValid = UserIDIsValid();

            if (userIDIsValid)
            {
                m_dbConnection.Open();
                string sql = "select tag, " +
                    "item, " + 
                    "thevalue, " +
                    "userid " +
                    "from tagitemvaltable " +
                    "where userid = '" + UserID + "'";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader query = command.ExecuteReader();

                TagItemValue temptiv;

                while (query.Read())
                {
                    temptiv = new TagItemValue();

                    temptiv._userid = UserID;
                    temptiv._tag = query.GetString(0);
                    temptiv._item = query.GetString(1);
                    temptiv._value = query.GetString(2);
                    temptiv._userid = query.GetValue(3).ToString();
                    _TIVManager._tivList.Add(temptiv);
                }

                m_dbConnection.Close();
            }
        }
             
        public List<object> GetEmailAddresses()
        {
            List<object> tempTagList = new List<object>();

            m_dbConnection.Open();
            string sql = "select emailaddress from emails where UserID=" + UserID + " order by emailaddress asc";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader query = command.ExecuteReader();

            while (query.Read())
            {
                string temp = query.GetString(0);
                tempTagList.Add(temp);
            }

            m_dbConnection.Close();

            return tempTagList;
        }
                 
        public void ResetTable(string tablename)
        {
            m_dbConnection.Open();
            string sql = "delete from " + tablename;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        public void ResetTableWhere(string tablename, 
            string where)
        {
            m_dbConnection.Open();
            string sql = "delete from " + tablename + " " + where;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        public void CreateUserTable()
        {
            m_dbConnection.Open();
            string sql = "CREATE TABLE IF NOT EXISTS 'user' ( 'UserName' TEXT NOT NULL, 'Password' TEXT NOT NULL, 'id' INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE )";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        public void CreateTagItemValueTable()
        {
            m_dbConnection.Open();
            string sql = "CREATE TABLE 'tagitemvaltable' ('tag'   TEXT, 'item'  TEXT, 'thevalue'  TEXT,'userid' INTEGER)CREATE TABLE IF NOT EXISTS 'user' ( 'UserName' TEXT NOT NULL, 'Password' TEXT NOT NULL, 'id' INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE )";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        public void CreatePreferencesTable()
        {
            m_dbConnection.Open();
            string sql = "CREATE TABLE 'preferences' ( 'UserID'    INTEGER,	'FromAddress'   TEXT,	'Port'  TEXT,	'UserName'  TEXT,	'Password'  TEXT,	'Host'  TEXT,	'SourceIP'  TEXT,	'SourcePort'    TEXT,	'DestinationIP' TEXT,	'DestinationPort'   TEXT)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        public void TruncateAllTables()
        {
            ResetTable("user");
            ResetTable("tagitemvaltable");
            ResetTable("preferences");
        }

        public void CreateAllTables()
        {
            CreateUserTable();
            CreatePreferencesTable();
            CreateTagItemValueTable();
        }

        public void CreateNewDatabase()
        {
            CreateAllTables();
        }          


        public string GetUserID(string tempUserName, 
            string tempPassword)
        {
            string UserID = EMPTYSTRING;
            UserName = tempUserName;
            Password = tempPassword;

            m_dbConnection.Open();
            
            string sql = "select id from user where UserName = '" + UserName + "' and Password = '" + Password + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader query = command.ExecuteReader();

            while (query.Read())
            {
                UserID = query[0].ToString();            
            }

            m_dbConnection.Close();               

            return UserID;
        }

        public object GetSingleRowValue(string tableName, 
            string columnName, 
            string where)
        {
            object theValue = null;

            m_dbConnection.Open();
            string sql = "select " + columnName + " from " + tableName + " " + where;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader query = command.ExecuteReader();

            while (query.Read())
            {
                theValue = query.GetValue(0);
            }

            m_dbConnection.Close();

            return theValue;
        }

        public void SaveEmailPreferencesToDatabase(string fromAddress,
            string host,
            string port,
            string UserName,
            string Password)
        {
            string sql = "";

            object tempuid = GetSingleRowValue("preferences", "UserID", "");

            m_dbConnection.Open();

            if (tempuid.ToString() == "")
            {
                sql = "insert into preferences (UserID, FromAddress, Port, UserName, Password, Host) values (" + UserID + ", '" +
                    DoubleUpQuotes(fromAddress) + "', '" +
                    DoubleUpQuotes(port) + "', '" +
                    DoubleUpQuotes(UserName) + "', '" +
                    DoubleUpQuotes(Password) + "', '" +
                    DoubleUpQuotes(host) + "')";
            }
            else
            {
                sql = "update preferences set " +
                 "FromAddress = '" + DoubleUpQuotes(fromAddress) + "', " +
                 "Port = '" + DoubleUpQuotes(port) + "', " +
                 "UserName = '" + DoubleUpQuotes(UserName) + "', " +
                 "Password = '" + DoubleUpQuotes(Password) + "', " +
                 "Host = '" + DoubleUpQuotes(host) + "' where UserID = " + UserID.ToString();
            }
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        public void SaveLANPreferencesToDatabase(string SourceIP,
           string SourcePort,
           string DestinationIP,
           string DestinationPort)
        {
            string sql = "";

            object tempuid = GetSingleRowValue("preferences", "UserID", "");

            m_dbConnection.Open();

            if (tempuid.ToString() == "")
            {
                sql = "insert into preferences (UserID, " +
                "SourceIP, " +
                "SourcePort, " +
                "DestinationIP," +
                "DestinationPort) values (" + UserID + ", '" +
                DoubleUpQuotes(SourceIP) + "', '" +
                DoubleUpQuotes(SourcePort) + "', '" +
                DoubleUpQuotes(DestinationIP) + "', '" +
                DoubleUpQuotes(DestinationPort) + "')";
            }
            else
            {
                sql = "update preferences set " +
                    "SourceIP = '" + DoubleUpQuotes(SourceIP) + "', " +
                    "SourcePort = '" + DoubleUpQuotes(SourcePort) + "', " +
                    "DestinationIP = '" + DoubleUpQuotes(DestinationIP) + "', " +
                    "DestinationPort = '" + DoubleUpQuotes(DestinationPort) + "' where UserID = " + UserID.ToString();
            }

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        public void SaveEmailToDatabase(string tempName, 
            string tempEmailAddress)
        {
            m_dbConnection.Open();
            string sql = "insert into emails (UserID, emailaddress) values (" + UserID + ", '" + DoubleUpQuotes(tempEmailAddress) + "')";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            m_dbConnection.Close();
        }

        public bool UserIDIsValid()
        {
            bool isvalid = false;

            if (UserID != null)
            {
                if(UserID.Trim() != "")
                {
                    if(UserID.Length >= 1)
                    {
                        if(Convert.ToInt32(UserID) > 0)
                        {
                            isvalid = true;
                        }
                    }
                }
            }

            return isvalid;
        }

        public void OverwriteTagItemsValueTableInDatabase()
        {
            bool userIDIsValid = UserIDIsValid();

            if (userIDIsValid)
            {
                //Helper.CreateCopyOfCurrentDB(TIVManager.GetTagItemValueCOUNT, "AUTO_BACKUP");
                m_dbConnection.Open();
                string sql = "delete from tagitemvaltable where userid = " + UserID;
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                               
                m_dbConnection.Close();

                m_dbConnection.Open();

                foreach (TagItemValue tiv in TIVManager._tivList)
                {
                    string temptag = (tiv._tag == null) ? null : DoubleUpQuotes(tiv._tag);
                    string tempitem = (tiv._item == null) ? null : DoubleUpQuotes(tiv._item);
                    string tempvalue = (tiv._value == null) ? null : DoubleUpQuotes(tiv._value);

                    sql = "insert into tagitemvaltable (tag, item, thevalue, userid) " +
                        "values ('" + temptag + "', " +
                        "'" + tempitem + "', " +
                        "'" + tempvalue + "', " +
                        "'" + UserID + "')";                        
                       
                        command = new SQLiteCommand(sql, m_dbConnection);
                        command.ExecuteNonQuery();                
                }

                m_dbConnection.Close();
            }
        }
    }
}