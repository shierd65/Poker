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
    public class UserController: Controller
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
                user.IsTaken = false;

                return RedirectToAction("Index", "Home");

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



            user = dal.Login(user.Username, user.Password);

            if (user == null)
            {
                user = new UserModel();
                user.LoginFail = true;
                return View("Login", user);
            }
            user.LoginFail = false;
            user.IsOnline = true;
            Session["user"] = user;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            UserModel user = new UserModel();
            user = (UserModel)Session["user"];
            user.IsOnline = false;
            Session.Abandon();
            
            return RedirectToAction("Index", "Home");
        }
    }
}