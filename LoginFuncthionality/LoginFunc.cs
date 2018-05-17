using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationDLL;
using SQLinfra;
using InfrastructureChat;


namespace LoginFuncthionality
{
    public class LoginFunc
    {
      
        public bool WriteUserToDbWithEncr(string loginName, string plainPassword, string email)
        {
            bool IsUserInDb = false;
           
            string pathToPublicKey = ConfigChatDll.OpenXmlPublicKeyPath();
            RSAEncryptDecrypt EncryptPassword = new RSAEncryptDecrypt();
            string cipherPassword = EncryptPassword.encryptRSA(plainPassword, pathToPublicKey);
            string dbConectionString = ConfigChatDll.OpenXmlConnectionString();

            SQLInfrastructure SqlWriteUserToDatabase = new SQLInfrastructure(dbConectionString);

            IsUserInDb = SqlWriteUserToDatabase.WriteToTable(loginName, email, cipherPassword);


            return IsUserInDb;

        }

        public bool IsUserCorrect(string loginName, string passwordUser)
        {
            bool isLoginAlreadyInDb = false;

           
            string dbConectionString = ConfigChatDll.OpenXmlConnectionString();
            SQLInfrastructure mySQLreadfromDBuser = new SQLInfrastructure(dbConectionString);
            isLoginAlreadyInDb = mySQLreadfromDBuser.IsloginIdInDataBAse(loginName);
            if (!isLoginAlreadyInDb) { return false; }

            RSAEncryptDecrypt EncryptPassword = new RSAEncryptDecrypt();

            string cypherText = mySQLreadfromDBuser.CipherPass(loginName);
            string plainPasswordfromDB;
            if (string.IsNullOrEmpty(cypherText))
            {
                
                return false;
            }

            plainPasswordfromDB = EncryptPassword.DecryptPassword(cypherText);
            bool dessision = plainPasswordfromDB.Equals(passwordUser);
           
            return dessision;


        }

        
       


    }
}
