﻿using Capstone.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Capstone.Web.Dal_s
{
    public class TableSqlDal
    {
        static string connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=poker;Persist Security Info=True;User ID=te_student;Password=techelevator";

        public Table FindTable(int tableID)
        {
            //bool foundTable = false;
            Table t = new Table();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT * from poker_table WHERE table_id = @tableID;", conn);
                    cmd.Parameters.AddWithValue("@tableID", tableID);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        t.Ante = Convert.ToInt32(reader["ante"]);
                        t.MaxBet = Convert.ToInt32(reader["max_bet"]);
                        t.MinBet = Convert.ToInt32(reader["min_bet"]);
                        t.TableHost = Convert.ToString(reader["host"]);
                        t.TableID = Convert.ToInt32(reader["table_id"]);
                        t.Name = Convert.ToString(reader["name"]);
                        t.MaxBuyIn = Convert.ToInt32(reader["max_buy_in"]);
                        t.Pot = Convert.ToInt32(reader["pot"]);
                        t.DealerPosition = Convert.ToInt32(reader["dealer_position"]);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return t;
        }

        public List<UserModel> GetAllPlayersAtTable(int tableID)
        {
            List<UserModel> output = new List<UserModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT table_players.player, users.current_money FROM table_players INNER JOIN users ON users.username = table_players.player WHERE table_id = @tableID;", conn);
                    cmd.Parameters.AddWithValue("@tableID", tableID);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        UserModel u = new UserModel();

                        u.CurrentMoney = Convert.ToInt32(reader["current_money"]);
                        u.Username = Convert.ToString(reader["player"]);

                        output.Add(u);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return output;
        }

        public int CreateTable(Table table)
        {
            int output = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO poker_table (name, host, max_bet, min_bet, ante, max_buy_in, pot, dealer_position) VALUES " +
                        "(@name, @host, @maxBet, @minBet, @ante, @maxBuyIn, @pot, @dealerPosition);", conn);
                    cmd.Parameters.AddWithValue("@name", table.Name);
                    cmd.Parameters.AddWithValue("@host", table.TableHost);
                    cmd.Parameters.AddWithValue("@maxBet", table.MaxBet);
                    cmd.Parameters.AddWithValue("@minBet", table.MinBet);
                    cmd.Parameters.AddWithValue("@ante", table.Ante);
                    cmd.Parameters.AddWithValue("@maxBuyIn", table.MaxBuyIn);
                    cmd.Parameters.AddWithValue("@pot", table.Pot);
                    cmd.Parameters.AddWithValue("@dealerPosition", table.DealerPosition);

                    //rowsAffected = cmd.ExecuteNonQuery();
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT table_id FROM poker_table WHERE name = @name ORDER BY table_id DESC;", conn);
                    cmd.Parameters.AddWithValue("@name", table.Name);

                    output = (int)cmd.ExecuteScalar();
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return output;
        }


        //not tested/used yet
        public List<Table> GetAllTables()
        {
            List<Table> output = new List<Table>();
            List<int> activeTables = new List<int>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SELECT UNIQUE table_id FROM table_players;");

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        activeTables.Add(Convert.ToInt32(reader["table_id"]));
                    }

                    foreach (int id in activeTables)
                    {
                        cmd.CommandText = "SELECT * FROM poker_table WHERE table_id = @id";
                        cmd.Parameters.AddWithValue("@id", id);

                        reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            Table t = new Table();
                            t.Ante = Convert.ToInt32(reader["ante"]);
                            t.MaxBet = Convert.ToInt32(reader["max_bet"]);
                            t.MinBet = Convert.ToInt32(reader["min_bet"]);
                            t.TableHost = Convert.ToString(reader["host"]);
                            t.TableID = Convert.ToInt32(reader["table_id"]);
                            t.Name = Convert.ToString(reader["name"]);

                            output.Add(t);
                        }
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return output;
        }


        //not tested/used yet
        public List<Card> GetAllCardsForPlayer(string username)
        {
            List<Card> output = new List<Card>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT card_number, card_suit FROM hand_cards WHERE player = @player;", conn);
                    cmd.Parameters.AddWithValue("@player", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Card c = new Card();

                        c.Number = Convert.ToInt32(reader["card_number"]);
                        c.Suit = Convert.ToString(reader["card_suit"]);

                        output.Add(c);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return output;
        }

        //not tested
        public void SetActivePlayer (string playerID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE table_players SET isTurn = 1 where player = @player;", conn);
                    cmd.Parameters.AddWithValue("@player", playerID);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

        }

        //not tested
        public string GetActivePlayer(int tableId)
        {
            string output = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("Select player from table_players where isTurn = 1 and table_id = @table_id;", conn);
                    cmd.Parameters.AddWithValue("@table_id", tableId);
                    output = cmd.ExecuteScalar().ToString();


                }
            }
            catch (SqlException)
            {
                throw;
            }
            return output;
        }

        //not yet tested
        //cannot accomodate one player two tables...possibly changed
        public void UpdateActivePlayer (int tableID, string playerID)
        {
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE table_players SET isTurn = 0 where table_id = @table_id and isTurn =1;", conn);
                    cmd.Parameters.AddWithValue("@table_id", tableID);
                    cmd.ExecuteNonQuery();

                    SqlCommand cmd1 = new SqlCommand("UPDATE table_players SET isTurn = 1 where table_id = @table_id and player = @player;", conn);
                    cmd1.Parameters.AddWithValue("@table_id", tableID);
                    cmd1.Parameters.AddWithValue("@player", playerID);
                    cmd1.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }           
        }

    }
}