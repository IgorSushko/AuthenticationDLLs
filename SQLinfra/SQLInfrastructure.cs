using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using BackEndMocker;


namespace SQLinfra
{
    public class SQLInfrastructure : IBrokerSQLCommunication
    {
        string ConnectionString;
        string passwordenc;
        
        int param1Key;
        int countrows = 0;
        bool ConnectionResult;

        
        public SQLInfrastructure(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }



        //*********************************************************************************************************
        /// <summary>
        /// Method requred for adding new user in system
        /// </summary>
        /// <param name="loginId">username</param>
        /// <param name="email">email for user</param>
        /// <param name="password">password for user</param>
        /// <returns>bool result of recording</returns>
        public bool WriteToTable(string loginId, string email, string password)
        {
            int exWriteToTable;
            int idWorker;
            bool WriteStatus = false;
            bool checkUserInDb = IsloginIdInDataBAse(loginId);
            if (checkUserInDb) {  return WriteStatus = false; }

            idWorker = IdWorkerCount();


                using (SqlConnection connWR = new SqlConnection())
                {
                    connWR.ConnectionString = ConnectionString;

                try
                {
                    connWR.Open();
                }
                catch (SqlException error) { throw error; }

                SqlCommand insertCommand = new SqlCommand("INSERT INTO tblLoginData (loginId, idWorker, email, password) VALUES (@0, @1, @2, @3)", connWR);

                    insertCommand.Parameters.Add(new SqlParameter("0", loginId));
                    insertCommand.Parameters.Add(new SqlParameter("1", ++idWorker));
                    insertCommand.Parameters.Add(new SqlParameter("2", email));
                    insertCommand.Parameters.Add(new SqlParameter("3", password));
                exWriteToTable = insertCommand.ExecuteNonQuery();
                    
                }
            
            
            
            return WriteStatus = true;
        }




        private int IdWorkerCount()
        {
            int idWorkerLastValue = 0;
            string idWorkerStr = String.Empty;
            
                using (SqlConnection conn = new SqlConnection())
                {

                    conn.ConnectionString = ConnectionString;

                try
                {
                    conn.Open();
                }
                catch (SqlException er)
                {

                    throw er;
                }

                SqlCommand command = new SqlCommand("SELECT[idWorker] FROM[teamChatDb].[dbo].[tblLoginData]", conn);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            idWorkerStr = (String.Format("{0}", reader[0]));

                        }

                    }
                }
                   

            if (string.IsNullOrEmpty(idWorkerStr)) { return idWorkerLastValue = 0; }
            else { idWorkerLastValue = Convert.ToInt32(idWorkerStr); }

            return idWorkerLastValue;
        }
        /// <summary>
        /// Using this method for verifying Is dedicated user in database
        /// </summary>
        /// <param name="loginId">Login name that need to be verified</param>
        /// <returns>true in case user already in Database, false - in case not</returns>
        public bool IsloginIdInDataBAse(string loginId)
        {
            bool loginIdInDataBAse = true;
            string loginIdInDataBAseStr = String.Empty;

            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConnectionString;
                try
                {
                    conn.Open();

                }
                catch (SqlException er)
                {
                    throw er;
                }

                SqlCommand command = new SqlCommand("SELECT [loginid] FROM [teamChatDb].[dbo].[tblLoginData] where loginid=" + "'" + loginId + "'", conn);

                
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        loginIdInDataBAseStr = (String.Format("{0}", reader[0]));

                    }

                }
            }
            if (string.IsNullOrEmpty(loginIdInDataBAseStr)) { return loginIdInDataBAse = false; }
            else { loginIdInDataBAse = true; }

            return loginIdInDataBAse;
        }

        //********************************************************************************************************
        /// <summary>
        /// This method return encrypted password for dedicated user
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns>strin - encrypted password</returns>
        public string CipherPass(string loginId)
        {
            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConnectionString;
                try
                {
                    conn.Open();

                }
                catch (SqlException er)
                {

                    throw er;
                }

                SqlCommand command = new SqlCommand("SELECT [password] FROM [teamChatDb].[dbo].[tblLoginData] where loginid=" + "'" + loginId + "'", conn);

                command.Parameters.Add(new SqlParameter("0", 1));

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {

                        passwordenc = (String.Format("{0}", reader[0]));
                        countrows++;

                    }
                    

                }
            }


            return passwordenc;
        }


        /// <summary>
        /// Required to get all registered users
        /// </summary>
        /// <returns>Returns the list of users from Database.</returns>
        public List<string> GetUserListFromDB()
        {
            List<string> userNammes = new List<string>();
            
            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConnectionString;
                try
                {
                    conn.Open();

                }
                catch (SqlException er)
                {

                    throw er;
                }

                SqlCommand command = new SqlCommand("SELECT [loginid] FROM [teamChatDb].[dbo].[tblLoginData]", conn);

                command.Parameters.Add(new SqlParameter("0", 1));

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        userNammes.Add(String.Format("{0}", reader[0]));

                    }

                }
            }
            
            return userNammes;
        }

        /// <summary>
        /// This methode create TopicId for ActiveMQ and assign it for dedicated users
        /// </summary>
        /// <param name="userOne"></param>
        /// <param name="userTwo"></param>
        /// <returns>TopicID int value</returns>
        public int GetTopicID(string userOne, string userTwo)
        {
            int getTopicValue;
            int IsRemovedFromTopic = 0;
            int userOneIdWorker;
            int userTwoIdWorker;
            List<int> userIdOnehasTopic = new List<int>();
            int userIdTwohasTopic;
            int exGetTopicID;
            int exGetTopicID2;

            userOneIdWorker = ProvideIdWorkerFromLogin(userOne);
            userTwoIdWorker = ProvideIdWorkerFromLogin(userTwo);
            userIdOnehasTopic = UserOneTopicsList(userOneIdWorker);
            userIdTwohasTopic = IsUserTwoHasSharedTopicWithUserOne(userIdOnehasTopic, userTwoIdWorker);

            if (userIdTwohasTopic != -1) { return getTopicValue = userIdTwohasTopic; }

            getTopicValue = CreateTopic();
            using (SqlConnection connWR = new SqlConnection())
            {
                connWR.ConnectionString = ConnectionString;

                connWR.Open();

                SqlCommand insertCommand = new SqlCommand("INSERT INTO tblUserPerTopics (idWorker, ID_column, IsRemovedFromTopic) VALUES (@0, @1, @2)", connWR);

                insertCommand.Parameters.Add(new SqlParameter("0", userOneIdWorker));
                insertCommand.Parameters.Add(new SqlParameter("1", getTopicValue));
                insertCommand.Parameters.Add(new SqlParameter("2", IsRemovedFromTopic));

                SqlCommand insertCommanduser2 = new SqlCommand("INSERT INTO tblUserPerTopics (idWorker, ID_column, IsRemovedFromTopic) VALUES (@0, @1, @2)", connWR);

                insertCommanduser2.Parameters.Add(new SqlParameter("0", userTwoIdWorker));
                insertCommanduser2.Parameters.Add(new SqlParameter("1", getTopicValue));
                insertCommanduser2.Parameters.Add(new SqlParameter("2", IsRemovedFromTopic));// 5- false  ; 255 -true

                exGetTopicID = insertCommand.ExecuteNonQuery();
                exGetTopicID2 = insertCommanduser2.ExecuteNonQuery();
                
            }

            return getTopicValue;
        }

        private List<int> UserOneTopicsList(int userID)
        {

            List<int> topicsList = new List<int>();

            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConnectionString;
                try
                {
                    conn.Open();

                }
                catch (SqlException er)
                {

                    throw er;
                }

                SqlCommand command = new SqlCommand("select ID_column from tblUserPerTopics where (idWorker=" + userID + " and IsRemovedFromTopic=0) ", conn);

                command.Parameters.Add(new SqlParameter("0", 1));

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        int intUserID = reader.GetInt32(0);
                        topicsList.Add(intUserID);
                    }

                }

            }

            return topicsList;
        }

        private int IsUserTwoHasSharedTopicWithUserOne(List<int> TopicIDs, int userTwoIdWorker)
        {

            int IsUserOneInTopic = -1;

            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConnectionString;
                try
                {
                    conn.Open();

                }
                catch (SqlException er)
                {

                    throw er;
                }

                foreach (int topicID in TopicIDs)
                {
                    SqlCommand command = new SqlCommand("select idWorker from tblUserPerTopics where (ID_column=" + topicID + " and IsRemovedFromTopic=0) ", conn);

                    command.Parameters.Add(new SqlParameter("0", 1));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            int intUserID = reader.GetInt32(0);
                            if (intUserID == userTwoIdWorker) { return IsUserOneInTopic = topicID; };
                        }

                    }
                }
            }

            return IsUserOneInTopic;
        }

        private int CreateTopic()
        {
            int topicIntValue;
            int exCreateTopic;
            string topicStrValue = String.Empty;
            DateTime dateCreated;

            dateCreated = DateTime.Now;

            using (SqlConnection connWR = new SqlConnection())
            {
                connWR.ConnectionString = ConnectionString;

                connWR.Open();

                SqlCommand insertCommand = new SqlCommand("Insert Into tblTopics (DateCreated) VALUES (@0)", connWR);
                insertCommand.Parameters.Add(new SqlParameter("0", dateCreated));
                exCreateTopic = insertCommand.ExecuteNonQuery();
               
            }

            using (SqlConnection connWR2 = new SqlConnection())
            {
                connWR2.ConnectionString = ConnectionString;

                connWR2.Open();
                SqlCommand command = new SqlCommand("select ID_column from tblTopics  where DateCreated="+"'"+ dateCreated+"'", connWR2);

                command.Parameters.Add(new SqlParameter("0", 1));

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        topicStrValue = (String.Format("{0}", reader[0]));

                    }

                }

            }
            if (string.IsNullOrEmpty(topicStrValue)) { return topicIntValue = -1; }
            else { topicIntValue = Convert.ToInt32(topicStrValue); }

            return topicIntValue;
        }

        /// <summary>
        /// Rreturn true in case of user existing in defined topic ID, false - in vice versa.
        /// </summary>
        /// <param name="userName">user identifier</param>
        /// <param name="topicId">id of topic</param>
        /// <returns>Result True in case user in Topic, and topicnot closed</returns>
        public bool IsUserExistingInChat(string userName, int topicId)
        {
            bool isUserexistResult = false;
            int userIdWorker;

            userIdWorker = ProvideIdWorkerFromLogin(userName);


            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConnectionString;
                try
                {
                    conn.Open();

                }
                catch (SqlException er)
                {

                    throw er;
                }


                SqlCommand command = new SqlCommand("select IsRemovedFromTopic from tblUserPerTopics where (idWorker=@username and ID_column=@topicid) ", conn);
                command.Parameters.Add(new SqlParameter("@username", userIdWorker));
                command.Parameters.Add(new SqlParameter("@topicid", topicId));


                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        int intIsUserInTopic = reader.GetInt32(0);
                        if (intIsUserInTopic == 0) { return isUserexistResult = true; };
                    }


                }
            }

            return isUserexistResult = false;
        }


        private int ProvideIdWorkerFromLogin(string loginId)
        {
            int loginIdInDataBAse;
            string loginIdInDataBAseStr = String.Empty;

            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConnectionString;
                try
                {
                    conn.Open();

                }
                catch (SqlException er)
                {

                    throw er;
                }

                SqlCommand command = new SqlCommand("SELECT [idWorker] FROM [teamChatDb].[dbo].[tblLoginData] where loginid=" + "'" + loginId + "'", conn);

                command.Parameters.Add(new SqlParameter("0", 1));

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        loginIdInDataBAseStr = (String.Format("{0}", reader[0]));

                    }

                }
            }
            if (string.IsNullOrEmpty(loginIdInDataBAseStr)) { return loginIdInDataBAse = -1; }
            else { loginIdInDataBAse = Convert.ToInt32(loginIdInDataBAseStr); }

            return loginIdInDataBAse;
        }

    }
}
