﻿using Capstone.Web.Crypto;
using Capstone.Web.Dal_s;
using Capstone.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Capstone.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserSqlDal dal;

        public UserController(IUserSqlDal dal)
        {
            this.dal = dal;
        }

        public ActionResult Register()
        {
            return View("Register", new UserModel());
        }

        [HttpPost]
        public ActionResult Register(UserModel user)
        {
            UserSqlDal dal = new UserSqlDal();
            if (ModelState.IsValid)
            {
                var newUser = new UserModel
                {
                    Username = user.Username,
                    Password = user.Password,
                };

                List<string> existingUsers = dal.GetAllUsernames();
                foreach (string name in existingUsers)
                {
                    if (name == user.Username)
                    {
                        user.IsTaken = true;
                        return View("Register", user);
                    }
                }

                var hashProvider = new HashProvider();
                user.Password = hashProvider.HashPassword(user.Password);
                user.Salt = hashProvider.SaltValue;

                dal.Register(user);
                Session["user"] = user;
                Session["username"] = user.Username;
                user.IsTaken = false;

                return RedirectToAction("LoggedInLanding", "Home");
            }
            else
            {
                return View("Register", user);
            }
        }

        public ActionResult Login()
        {
            return View("Login", new UserModel());
        }

        [HttpPost]
        public ActionResult Login(UserModel user)
        {
            UserSqlDal dal = new UserSqlDal();
            UserModel existingUser = dal.Login(user.Username);

            if (existingUser == null)
            {
                user.LoginFail = true;
                return View("Login", user);
            }

            HashProvider provider = new HashProvider();

            if (user.LoginPassword != null && provider.VerifyPasswordMatch(existingUser.Password, user.LoginPassword, existingUser.Salt))
            {
                user.LoginFail = false;
                user.IsOnline = true;
                Session["user"] = user;
                Session["username"] = user.Username;
                return RedirectToAction("LoggedInLanding", "Home");
            }

            user.LoginFail = true;
            return View("Login", user);
        }

        public ActionResult Logout()
        {
            UserModel user = new UserModel();
            user = (UserModel)Session["user"];

            string username = (string)Session["username"];

            UserSqlDal dal = new UserSqlDal();
            bool inGame = dal.CheckStatus(username);

            if (inGame)
            {
                return RedirectToAction("LeaveTable", "Table");
            }

            if (user != null)
            {
                user.IsOnline = false;
                Session.Abandon();
            }

            return RedirectToAction("Index", "Home");
        }

        public bool IsAuthenticated
        {
            get
            {
                return Session["user"] != null;
            }
        }

        [ChildActionOnly]
        public ActionResult GetAuthenticatedUser()
        {
            UserModel model = (UserModel)Session["user"];

            if (IsAuthenticated)
            {
                model = dal.Login(model.Username);
            }

            return PartialView("_NavBar", model);
        }

        //top 10 most current chips

        public ActionResult TopScores()
        {
            UserSqlDal dal = new UserSqlDal();
            Dictionary<string, int> TopScores = dal.GetAllUsernamesWithChipsSortedByChipCount();

            return View("TopScores", TopScores);
        }

        public ActionResult MoneyReset()
        {
            string username = (string)Session["username"];

            UserSqlDal dal = new UserSqlDal();
            dal.UpdateMoney(username, 1000);

            return View("MoneyReset");
        }
    }
}