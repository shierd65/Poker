﻿using System;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using Capstone.Web.Dal_s;
using Capstone.Web.Models;

namespace Capstone.Web.Tests.DALTest
{
    [TestClass]
    public class UserSqlDALDTest
    {
        private TransactionScope tran;
        string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=poker;Persist Security Info=True;User ID=te_student;Password=techelevator";


        [TestInitialize]
        public void Initialize()
        {
            tran = new TransactionScope();
            int rowsAffected;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                //this will need to be updated when we add tables that depend on users
                SqlCommand cmd = new SqlCommand("DELETE FROM table_players; DELETE FROM hand_actions; DELETE FROM hand_cards; " +
                    "DELETE FROM hand; DELETE FROM poker_table; DELETE FROM users;", conn);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("INSERT INTO users VALUES" +
                    "('Brian', 'pwd', 500, 2000, 'admin', 0, 'LOL__WUT'), " +
                    "('Bob', 'pwd2', 50000, 50020, 'admin', 1, '8675309z'), " +
                    "('Boo', 'Hoo', 1000, 1000, 'player', 0, 'omg-hash') "
                    , conn);
                //cmd = new SqlCommand("INSERT INTO users VALUES((" +
                //     "'Brian', 'pwd', 500, 2000, 'admin', 0);", conn);


                rowsAffected = cmd.ExecuteNonQuery();
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            tran.Dispose();
        }

        [TestMethod]
        public void TestGetAllUsernames()
        {
            UserSqlDal dal = new UserSqlDal();
            List<string> output = dal.GetAllUsernames();

            Assert.AreEqual(3, output.Count);
            CollectionAssert.Contains(output, "Bob");
            CollectionAssert.Contains(output, "Boo");
            CollectionAssert.DoesNotContain(output, "Boa");
        }
        [TestMethod]
        public void TestGetTop10UserNamesWithChipCounts()
        {
            UserSqlDal dal = new UserSqlDal();
            Dictionary<string, int> output = dal.GetAllUsernamesWithChipsSortedByChipCount();
            Assert.AreEqual(3, output.Count);
            //CollectionAssert.Contains(output, "Boo");
            Assert.AreEqual(50000, output["Bob"]);
            Assert.AreEqual(500, output["Brian"]);
            CollectionAssert.DoesNotContain(output.Keys, "Boa");
        }


        [TestMethod]
        public void TestAddUser()
        {
            UserModel newUser = new UserModel();
            newUser.Username = "Boa";
            newUser.Password = "aaa";
            newUser.ConfirmPassword = "aaa";
            newUser.CurrentMoney = 9001;
            newUser.HighestMoney = 9001;
            newUser.IsOnline = true;
            newUser.Privilege = "duck";
            newUser.IsTaken = false;
            newUser.LoginFail = false;
            //add salt that matches their formatting
            //TEST THIS TOMORROW
            newUser.Salt = "UBROKEIT";

            UserSqlDal dal = new UserSqlDal();
            bool confirm = dal.Register(newUser);

            Assert.AreEqual(true, confirm);

            List<UserModel> allUsers = new List<UserModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM users ORDER BY username DESC;", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    UserModel u = new UserModel();
                    u.Username = Convert.ToString(reader["username"]);
                    u.Password = Convert.ToString(reader["password"]);
                    u.CurrentMoney = Convert.ToInt32(reader["current_money"]);
                    u.HighestMoney = Convert.ToInt32(reader["highest_money"]);
                    u.Privilege = Convert.ToString(reader["privilege"]);
                    u.IsOnline = Convert.ToBoolean(reader["is_online"]);
                    u.Salt = Convert.ToString(reader["salt"]);
                    allUsers.Add(u);
                }

                Assert.IsNotNull(allUsers);
                Assert.AreEqual(4, allUsers.Count);
                Assert.AreEqual("Boa", allUsers[3].Username);
                Assert.AreEqual("aaa", allUsers[3].Password);
                Assert.AreEqual(50000, allUsers[2].CurrentMoney);
                Assert.AreEqual("omg-hash", allUsers[1].Salt);
            }

        }

        [TestMethod]
        public void TestLoginSuccess()
        {
            UserSqlDal dal = new UserSqlDal();
            UserModel a = dal.Login("Bob");

            Assert.AreEqual(50000, a.CurrentMoney);
            Assert.AreEqual("admin", a.Privilege);
            Assert.AreEqual("pwd2", a.Password);
        }

        //this kinda made more sense when we were checking password in the DAL
        [TestMethod]
        public void TestLoginFail()
        {
            UserSqlDal dal = new UserSqlDal();
            UserModel a = dal.Login("Boa");
            UserModel b = dal.Login("arglefargle");
            UserModel c = dal.Login("Bo");
            Assert.IsNull(a);
            Assert.IsNull(b);
            Assert.IsNull(c);
            //Assert.AreNotEqual(50000, a.CurrentMoney);
            //Assert.AreNotEqual("admin", a.Privilege);
        }
    }
}
