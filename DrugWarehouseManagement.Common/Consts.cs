using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Common
{
    public static class Consts
    {
        public const string htmlCreateAccountTemplate = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            margin: 0;
                            padding: 0;
                        }

                        .email-container {
                            max-width: 600px;
                            margin: 20px auto;
                            background: #ffffff;
                            border: 1px solid #dddddd;
                            border-radius: 10px;
                            overflow: hidden;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                        }

                        .email-header {
                            background-color: #4CAF50;
                            color: white;
                            text-align: center;
                            padding: 20px;
                            font-size: 24px;
                            font-weight: bold;
                        }

                        .email-body {
                            padding: 20px;
                            line-height: 1.6;
                            color: #333333;
                        }

                        .email-body p {
                            margin: 10px 0;
                        }

                        .email-footer {
                            background-color: #f4f4f4;
                            text-align: center;
                            padding: 10px;
                            font-size: 12px;
                            color: #777777;
                        }

                        .button {
                            display: inline-block;
                            background-color: #4CAF50;
                            color: white !important;
                            text-decoration: none;
                            padding: 10px 20px;
                            border-radius: 5px;
                            font-weight: bold;
                            margin-top: 10px;
                        }

                        .button:hover {
                            background-color: #45a049;
                        }
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            Welcome to Our Platform!
                        </div>
                        <div class='email-body'>
                            <p>Hi <strong>{{Username}}</strong>,</p>
                            <p>Your account has been successfully created. Below are your login credentials:</p>
                            <ul>
                                <li><strong>Username:</strong> {{Username}}</li>
                                <li><strong>Password:</strong> {{Password}}</li>
                            </ul>
                            <p>Please make sure to change your password after your first login.</p>
                            <a href='https://yourwebsite.com/login' class='button'>Login to Your Account</a>
                            <p>If you have any questions or need assistance, feel free to contact our support team.</p>
                        </div>
                        <div class='email-footer'>
                            &copy; 2025 Your Company. All rights reserved.<br>
                            Please do not reply to this email.
                        </div>
                    </div>
                </body>
                </html>";
        public const string htmlResetPasswordTemplate = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            margin: 0;
                            padding: 0;
                        }

                        .email-container {
                            max-width: 600px;
                            margin: 20px auto;
                            background: #ffffff;
                            border: 1px solid #dddddd;
                            border-radius: 10px;
                            overflow: hidden;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                        }

                        .email-header {
                            background-color: #4CAF50;
                            color: white;
                            text-align: center;
                            padding: 20px;
                            font-size: 24px;
                            font-weight: bold;
                        }

                        .email-body {
                            padding: 20px;
                            line-height: 1.6;
                            color: #333333;
                        }

                        .email-body p {
                            margin: 10px 0;
                        }

                        .email-footer {
                            background-color: #f4f4f4;
                            text-align: center;
                            padding: 10px;
                            font-size: 12px;
                            color: #777777;
                        }

                        .button {
                            display: inline-block;
                            background-color: #4CAF50;
                            color: white !important;
                            text-decoration: none;
                            padding: 10px 20px;
                            border-radius: 5px;
                            font-weight: bold;
                            margin-top: 10px;
                        }

                        .button:hover {
                            background-color: #45a049;
                        }
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            Welcome to Our Platform!
                        </div>
                        <div class='email-body'>
                            <p>Hi <strong>{{Username}}</strong>,</p>
                            <p>You have requested reset password for your account. Below are your new login credentials:</p>
                            <ul>
                                <li><strong>Username:</strong> {{Username}}</li>
                                <li><strong>Password:</strong> {{Password}}</li>
                            </ul>
                            <p>Please make sure to change your password after your first login.</p>
                            <a href='https://yourwebsite.com/login' class='button'>Login to Your Account</a>
                            <p>If you have any questions or need assistance, feel free to contact our support team.</p>
                        </div>
                        <div class='email-footer'>
                            &copy; 2025 Your Company. All rights reserved.<br>
                            Please do not reply to this email.
                        </div>
                    </div>
                </body>
                </html>";

    }
}
