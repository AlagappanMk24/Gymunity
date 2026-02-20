using Gymunity.Application.Contracts.ExternalServices.Email;

namespace Gymunity.Infrastructure.ExternalServices.Email.Template
{
    public class EmailTemplateRenderer : IEmailTemplateRenderer
    {
        /// <summary>
        /// Login Confirmation Email Template
        /// </summary>
        public string GetLoginConfirmationEmail(string userName, string loginTime, string location, string device)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <style>
                    body, html {{
                        margin: 0;
                        padding: 0;
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        -webkit-font-smoothing: antialiased;
                        -moz-osx-font-smoothing: grayscale;
                    }}
        
                    /* Main container */
                    .container {{
                        max-width: 600px;
                        margin: 20px auto;
                        background-color: #ffffff;
                        border-radius: 20px;
                        overflow: hidden;
                        box-shadow: 0 20px 40px rgba(0,0,0,0.15);
                    }}
        
                    /* Header section */
                    .header {{
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        padding: 40px 30px;
                        text-align: center;
                        color: white;
                    }}
        
                    .brand-name {{
                        font-size: 28px;
                        font-weight: 800;
                        letter-spacing: 3px;
                        display: block;
                        margin-bottom: 10px;
                        text-transform: uppercase;
                        text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
                    }}
        
                    .header h1 {{
                        margin: 0;
                        font-size: 28px;
                        font-weight: 500;
                    }}
        
                    /* Content section */
                    .content {{
                        padding: 30px;
                        background: #f8fafc;
                    }}
        
                    /* Success heading */
                    .success-heading {{
                        color: #667eea;
                        text-align: center;
                        font-size: 24px;
                        margin: 0 0 20px 0;
                    }}
        
                    /* Info card */
                    .info-card {{
                        background: white;
                        border-radius: 16px;
                        padding: 25px;
                        margin: 20px 0;
                        border-left: 5px solid #667eea;
                        box-shadow: 0 5px 20px rgba(102,126,234,0.1);
                    }}
        
                    .greeting {{
                        font-size: 16px;
                        color: #333;
                        margin-top: 0;
                        margin-bottom: 20px;
                        line-height: 1.5;
                    }}
        
                    /* Details box */
                    .details-box {{
                        background: #f0f4ff;
                        border-radius: 12px;
                        padding: 20px;
                        font-size: 15px;
                        line-height: 1.8;
                    }}
        
                    .detail-item {{
                        display: flex;
                        align-items: flex-start;
                        margin-bottom: 12px;
                    }}
        
                    .detail-icon {{
                        width: 30px;
                        font-size: 18px;
                        display: inline-block;
                    }}
        
                    .detail-content {{
                        flex: 1;
                    }}
        
                    .detail-label {{
                        font-weight: 600;
                        color: #4a5568;
                        margin-right: 8px;
                    }}
        
                    .detail-value {{
                        color: #2d3748;
                    }}
        
                    /* Button */
                    .button-container {{
                        text-align: center;
                        margin: 30px 0 20px;
                    }}
        
                    .button {{
                        display: inline-block;
                        padding: 16px 40px;
                        background: linear-gradient(135deg, #667eea, #764ba2);
                        color: white !important;
                        text-decoration: none;
                        border-radius: 50px;
                        font-weight: 600;
                        font-size: 16px;
                        box-shadow: 0 10px 20px rgba(102,126,234,0.3);
                        transition: transform 0.3s;
                    }}
        
                    .button:hover {{
                        transform: translateY(-2px);
                    }}
        
                    /* Divider */
                    .divider {{
                        border: none;
                        border-top: 2px dashed #cbd5e0;
                        margin: 30px 0;
                    }}
        
                    /* Footer section */
                    .footer-content {{
                        text-align: center;
                        color: #718096;
                        font-size: 13px;
                        line-height: 1.6;
                        padding: 0 20px 20px;
                    }}
        
                    .heart {{
                        color: #f56565;
                        font-size: 16px;
                    }}
        
                    .support-link {{
                        color: #667eea;
                        text-decoration: none;
                        font-weight: 600;
                        margin-top: 5px;
                        display: inline-block;
                    }}
        
                    .support-link:hover {{
                        text-decoration: underline;
                    }}
        
                    .copyright {{
                        margin-top: 15px;
                        color: #a0aec0;
                        font-size: 12px;
                    }}
        
                    /* Mobile responsive */
                    @media only screen and (max-width: 480px) {{
                        .container {{
                            margin: 10px;
                            border-radius: 16px;
                        }}
            
                        .header {{
                            padding: 30px 20px;
                        }}
            
                        .brand-name {{
                            font-size: 24px;
                        }}
            
                        .header h1 {{
                            font-size: 22px;
                        }}
            
                        .content {{
                            padding: 20px;
                        }}
            
                        .success-heading {{
                            font-size: 20px;
                        }}
            
                        .info-card {{
                            padding: 20px;
                        }}
            
                        .details-box {{
                            padding: 15px;
                            font-size: 14px;
                        }}
            
                        .detail-item {{
                            flex-direction: column;
                            margin-bottom: 15px;
                        }}
            
                        .detail-icon {{
                            width: auto;
                            margin-bottom: 5px;
                        }}
            
                        .button {{
                            padding: 14px 30px;
                            font-size: 14px;
                            display: block;
                            width: 100%;
                            box-sizing: border-box;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <!-- Header -->
                    <div class=""header"">
                        <span class=""brand-name"">GYMUNITY</span>
                        <h1>🔐 Welcome Back!</h1>
                    </div>
        
                    <!-- Main Content -->
                    <div class=""content"">
                        <h2 class=""success-heading"">Login Successful! 🎉</h2>
            
                        <div class=""info-card"">
                            <p class=""greeting"">Hi <strong>{userName}</strong>, we noticed a login to your account:</p>
                
                            <div class=""details-box"">
                                <div class=""detail-item"">
                                    <span class=""detail-icon"">📅</span>
                                    <span class=""detail-content"">
                                        <span class=""detail-label"">Time:</span>
                                        <span class=""detail-value"">{loginTime}</span>
                                    </span>
                                </div>
                    
                                <div class=""detail-item"">
                                    <span class=""detail-icon"">🌍</span>
                                    <span class=""detail-content"">
                                        <span class=""detail-label"">Location:</span>
                                        <span class=""detail-value"">{location}</span>
                                    </span>
                                </div>
                    
                                <div class=""detail-item"">
                                    <span class=""detail-icon"">💻</span>
                                    <span class=""detail-content"">
                                        <span class=""detail-label"">Device:</span>
                                        <span class=""detail-value"">{device}</span>
                                    </span>
                                </div>
                            </div>
                        </div>
            
                        <div class=""button-container"">
                            <a href=""https://gymunity.com/dashboard"" class=""button"">🚀 Go to Dashboard</a>
                        </div>
            
                        <hr class=""divider"">
            
                        <!-- Footer with proper alignment -->
                        <div class=""footer-content"">
                            <p>
                                Sent with <span class=""heart"">❤️</span> from the Gymunity Team
                            </p>
                            <a href=""mailto:support@gymunity.com"" class=""support-link"">
                                📧 Contact Support
                            </a>
                            <div class=""copyright"">
                                &copy; 2026 Gymunity. All rights reserved.
                            </div>
                        </div>
                    </div>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// Registration Confirmation Email Template
        /// </summary>
        public string GetRegistrationConfirmationEmail(string userName)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <style>
                    /* Reset styles */
                    body, html {{
                        margin: 0;
                        padding: 0;
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        background: #f0f2f5;
                        -webkit-font-smoothing: antialiased;
                    }}

                    .container {{
                        max-width: 600px;
                        margin: 20px auto;
                        background-color: #ffffff;
                        border-radius: 20px;
                        overflow: hidden;
                        box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                    }}

                    .header {{
                        background: linear-gradient(135deg, #ff6b6b 0%, #feca57 100%);
                        padding: 40px 20px;
                        text-align: center;
                        color: white;
                    }}

                    .brand-name {{
                        font-size: 28px;
                        font-weight: 800;
                        letter-spacing: 3px;
                        display: block;
                        text-transform: uppercase;
                    }}

                    .content {{
                        padding: 30px;
                        background: #f8fafc;
                        text-align: center; /* Centers the inline-block items */
                    }}

                    /* Updated Feature Grid for Mobile Support */
                    .feature-grid {{
                        text-align: center;
                        font-size: 0; /* Removes whitespace between inline-blocks */
                        margin: 20px 0;
                    }}

                    .feature-item {{
                        display: inline-block;
                        vertical-align: top;
                        background: #fff5f5;
                        padding: 15px 10px;
                        margin: 5px;
                        border-radius: 12px;
                        width: 45%; /* Two columns on desktop */
                        min-width: 140px;
                        box-sizing: border-box;
                        font-size: 14px; /* Reset font size */
                    }}

                    .feature-icon {{ font-size: 20px; display: block; margin-bottom: 5px; }}
                    .feature-text {{ font-weight: 600; color: #4a5568; }}

                    .button-container {{
                            text-align: center;
                            margin: 30px 0 20px;
                     }}
                   
                        .button {{
                            display: inline-block;
                            padding: 16px 40px;
                            background: linear-gradient(135deg, #ff6b6b, #feca57);
                            color: white !important;
                            text-decoration: none;
                            border-radius: 50px;
                            font-weight: 600;
                            font-size: 16px;
                            box-shadow: 0 10px 20px rgba(255,107,107,0.3);
                            transition: transform 0.3s;
                        }}

                        .button:hover {{
                            transform: translateY(-2px);
                        }}

                        /* Divider */
                        .divider {{
                            border: none;
                            border-top: 2px dashed #fed7d7;
                            margin: 30px 0;
                        }}

                        /* Footer section */
                        .footer-content {{
                            text-align: center;
                            color: #718096;
                            font-size: 13px;
                            line-height: 1.6;
                            padding: 0 20px 20px;
                        }}

                        .heart {{
                            color: #f56565;
                            font-size: 16px;
                        }}

                        .support-link {{
                            color: #ff6b6b;
                            text-decoration: none;
                            font-weight: 600;
                            margin-top: 5px;
                            display: inline-block;
                        }}

                        .support-link:hover {{
                            text-decoration: underline;
                        }}

                        .copyright {{
                            margin-top: 15px;
                            color: #a0aec0;
                            font-size: 12px;
                        }}


                    /* Mobile Overrides */
                    @media only screen and (max-width: 480px) {{
                            .container {{ margin: 10px; border-radius: 16px;}}
                            .header {{
                                padding: 30px 20px;
                            }}

                            .brand-name {{
                                font-size: 24px;
                            }}

                            .header h1 {{
                                font-size: 22px;
                            }}

                            .content {{
                                padding: 20px;
                            }}

                            .greeting {{
                                font-size: 20px;
                            }}

                            .feature-grid {{
                                gap: 8px;
                            }}

                            .feature-item {{
                                width: 100% !important; /* Stack full width on mobile */
                                margin: 5px 0 !important;
                            }}
                            .button {{
                                padding: 14px 30px;
                                font-size: 14px;
                                display: block;
                                width: 100%;
                                box-sizing: border-box;
                            }}
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <span class=""brand-name"">GYMUNITY</span>
                        <h1 style=""margin-top:10px;"">🎉 Welcome to the Family!</h1>
                    </div>

                    <div class=""content"">
                        <h2 style=""color:#ff6b6b;"">Hello {userName}! 👋</h2>
                        <p>You're now part of the most exciting fitness community.</p>

                        <div class=""feature-grid"">
                            <div class=""feature-item"">
                                <span class=""feature-icon"">📋</span>
                                <span class=""feature-text"">Custom Plans</span>
                            </div>
                            <div class=""feature-item"">
                                <span class=""feature-icon"">🎥</span>
                                <span class=""feature-text"">Video Library</span>
                            </div>
                            <div class=""feature-item"">
                                <span class=""feature-icon"">📊</span>
                                <span class=""feature-text"">Progress Tracker</span>
                            </div>
                            <div class=""feature-item"">
                                <span class=""feature-icon"">👥</span>
                                <span class=""feature-text"">Community</span>
                            </div>
                        </div>

                         <div class=""button-container"">
                                <a href=""https://gymunity.com/get-started"" class=""button"">🌟 START YOUR JOURNEY</a>
                         </div>
                        
                         <hr class=""divider"">

                            <!-- Footer with proper alignment -->
                            <div class=""footer-content"">
                                <p>
                                    Sent with <span class=""heart"">❤️</span> from the Gymunity Team
                                </p>
                                <a href=""mailto:support@gymunity.com"" class=""support-link"">
                                    📧 Contact Support
                                </a>
                                <div class=""copyright"">
                                    &copy; 2026 Gymunity. All rights reserved.
                                </div>
                            </div>
                    </div>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// OTP Verification Email Template
        /// </summary>
        public string GetOtpVerificationEmail(string otpCode, string purpose)
        {
            string purposeText = purpose.ToLower() switch
            {
                "register" => "complete your registration",
                "login" => "log in to your account",
                "reset-password" => "reset your password",
                "change-email" => "change your email",
                _ => "verify your identity"
            };

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""utf-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        /* Reset styles */
                        body, html {{
                            margin: 0;
                            padding: 0;
                            font-family: 'Poppins', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background: linear-gradient(135deg, #4158D0 0%, #C850C0 46%, #FFCC70 100%);
                            -webkit-font-smoothing: antialiased;
                            -moz-osx-font-smoothing: grayscale;
                        }}
        
                        /* Main container */
                        .container {{
                            max-width: 550px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border-radius: 20px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(65,88,208,0.3);
                        }}
        
                        /* Header section */
                        .header {{
                            background: linear-gradient(135deg, #4158D0 0%, #C850C0 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
        
                        .brand-name {{
                            font-size: 28px;
                            font-weight: 800;
                            letter-spacing: 3px;
                            display: block;
                            margin-bottom: 10px;
                            text-transform: uppercase;
                            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
                        }}
        
                        .header h1 {{
                            margin: 0;
                            font-size: 24px;
                            font-weight: 400;
                            opacity: 0.9;
                        }}
        
                        /* Content section */
                        .content {{
                            padding: 30px;
                            background: #f8fafc;
                        }}
        
                        /* OTP card */
                        .otp-card {{
                            background: white;
                            border-radius: 16px;
                            padding: 25px;
                            margin: 20px 0;
                            border-left: 5px solid #4158D0;
                            box-shadow: 0 5px 20px rgba(65,88,208,0.1);
                            text-align: center;
                        }}
        
                        .otp-label {{
                            color: #4158D0;
                            font-size: 18px;
                            margin: 0 0 10px 0;
                        }}
        
                        .otp-code {{
                            font-size: 48px;
                            font-weight: 700;
                            letter-spacing: 15px;
                            color: #4158D0;
                            margin: 20px 0;
                            padding: 15px;
                            background: #f0f4ff;
                            border-radius: 12px;
                            word-break: break-all;
                        }}
        
                        .purpose-text {{
                            font-size: 16px;
                            color: #333;
                            margin-bottom: 20px;
                            line-height: 1.5;
                        }}
        
                        /* Timer box */
                        .timer-box {{
                            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
                            border-radius: 12px;
                            padding: 20px;
                            margin: 20px 0;
                            color: white;
                            text-align: center;
                        }}
        
                        .timer-text {{
                            font-size: 18px;
                            font-weight: 500;
                            margin: 0;
                        }}
        
                        .timer-icon {{
                            font-size: 24px;
                            margin-right: 10px;
                            vertical-align: middle;
                        }}
        
                        /* Security tip */
                        .security-tip {{
                            background: #fff3cd;
                            border-left: 5px solid #ffc107;
                            border-radius: 12px;
                            padding: 20px;
                            margin: 20px 0;
                            color: #856404;
                        }}
        
                        .security-tip p {{
                            margin: 0;
                            font-size: 14px;
                        }}
        
                        /* Divider */
                        .divider {{
                            border: none;
                            border-top: 2px dashed #cbd5e0;
                            margin: 30px 0;
                        }}
        
                        /* Footer section */
                        .footer-content {{
                            text-align: center;
                            color: #718096;
                            font-size: 13px;
                            line-height: 1.6;
                            padding: 0 20px 20px;
                        }}
        
                        .heart {{
                            color: #f56565;
                            font-size: 16px;
                        }}
        
                        .support-link {{
                            color: #4158D0;
                            text-decoration: none;
                            font-weight: 600;
                            margin-top: 5px;
                            display: inline-block;
                        }}
        
                        .support-link:hover {{
                            text-decoration: underline;
                        }}
        
                        .copyright {{
                            margin-top: 15px;
                            color: #a0aec0;
                            font-size: 12px;
                        }}
        
                        /* Mobile responsive */
                        @media only screen and (max-width: 480px) {{
                            .container {{
                                margin: 10px;
                                border-radius: 16px;
                            }}
            
                            .header {{
                                padding: 30px 20px;
                            }}
            
                            .brand-name {{
                                font-size: 24px;
                            }}
            
                            .header h1 {{
                                font-size: 18px;
                            }}
            
                            .content {{
                                padding: 20px;
                            }}
            
                            .otp-code {{
                                font-size: 32px;
                                letter-spacing: 8px;
                                padding: 10px;
                            }}
            
                            .timer-text {{
                                font-size: 16px;
                            }}
            
                            .security-tip {{
                                padding: 15px;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <!-- Header -->
                        <div class=""header"">
                            <span class=""brand-name"">GYMUNITY</span>
                            <h1>Verify Your Identity</h1>
                        </div>
        
                        <!-- Main Content -->
                        <div class=""content"">
                            <div class=""otp-card"">
                                <h3 class=""otp-label"">Your Verification Code</h3>
                                <p class=""purpose-text"">Use this code to {purposeText}:</p>
                
                                <div class=""otp-code"">{otpCode}</div>
                
                                <div class=""timer-box"">
                                    <span class=""timer-icon"">⏰</span>
                                    <span class=""timer-text"">Code expires in 5 minutes</span>
                                </div>
                
                                <div class=""security-tip"">
                                    <p><strong>🔒 Never share this code with anyone.</strong> Gymunity will never ask for your OTP via phone or email.</p>
                                </div>
                            </div>
            
                            <hr class=""divider"">
            
                            <!-- Footer with proper alignment -->
                            <div class=""footer-content"">
                                <p>
                                    Sent with <span class=""heart"">❤️</span> from the Gymunity Team
                                </p>
                                <a href=""mailto:support@gymunity.com"" class=""support-link"">
                                    📧 Contact Support
                                </a>
                                <div class=""copyright"">
                                    &copy; 2026 Gymunity. All rights reserved.
                                </div>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        /// <summary>
        /// Reset Password Link Email Template
        /// </summary>
        public string GetResetPasswordLinkEmail(string userName, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""utf-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        /* Reset styles */
                        body, html {{
                            margin: 0;
                            padding: 0;
                            font-family: 'Montserrat', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background: linear-gradient(135deg, #134E5E 0%, #71B280 100%);
                            -webkit-font-smoothing: antialiased;
                            -moz-osx-font-smoothing: grayscale;
                        }}
        
                        /* Main container */
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border-radius: 20px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(19,78,94,0.3);
                        }}
        
                        /* Header section */
                        .header {{
                            background: linear-gradient(135deg, #134E5E 0%, #71B280 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
        
                        .brand-name {{
                            font-size: 28px;
                            font-weight: 800;
                            letter-spacing: 3px;
                            display: block;
                            margin-bottom: 10px;
                            text-transform: uppercase;
                            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
                        }}
        
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: 500;
                        }}
        
                        /* Content section */
                        .content {{
                            padding: 30px;
                            background: #f8fafc;
                        }}
        
                        /* Reset card */
                        .reset-card {{
                            background: white;
                            border-radius: 16px;
                            padding: 25px;
                            margin: 20px 0;
                            border-left: 5px solid #134E5E;
                            box-shadow: 0 5px 20px rgba(19,78,94,0.1);
                        }}
        
                        .greeting {{
                            font-size: 24px;
                            color: #134E5E;
                            margin: 0 0 10px 0;
                        }}
        
                        .message {{
                            font-size: 16px;
                            color: #333;
                            margin-bottom: 20px;
                            line-height: 1.5;
                        }}
        
                        /* Expiry badge */
                        .expiry-badge {{
                            background: #ff6b6b;
                            color: white;
                            padding: 8px 20px;
                            border-radius: 50px;
                            display: inline-block;
                            font-weight: 600;
                            font-size: 14px;
                            margin: 20px 0;
                        }}
        
                        /* Button */
                        .button-container {{
                            text-align: center;
                            margin: 30px 0 20px;
                        }}
        
                        .button {{
                            display: inline-block;
                            padding: 16px 40px;
                            background: linear-gradient(135deg, #134E5E, #71B280);
                            color: white !important;
                            text-decoration: none;
                            border-radius: 50px;
                            font-weight: 600;
                            font-size: 16px;
                            box-shadow: 0 10px 20px rgba(19,78,94,0.3);
                            transition: transform 0.3s;
                        }}
        
                        .button:hover {{
                            transform: translateY(-2px);
                        }}
        
                        /* Warning box */
                        .warning-box {{
                            background: #fff3cd;
                            border-left: 5px solid #ffc107;
                            border-radius: 12px;
                            padding: 20px;
                            margin: 20px 0;
                            color: #856404;
                        }}
        
                        .warning-box p {{
                            margin: 0;
                            font-size: 14px;
                        }}
        
                        /* Divider */
                        .divider {{
                            border: none;
                            border-top: 2px dashed #cbd5e0;
                            margin: 30px 0;
                        }}
        
                        /* Footer section */
                        .footer-content {{
                            text-align: center;
                            color: #718096;
                            font-size: 13px;
                            line-height: 1.6;
                            padding: 0 20px 20px;
                        }}
        
                        .heart {{
                            color: #f56565;
                            font-size: 16px;
                        }}
        
                        .support-link {{
                            color: #134E5E;
                            text-decoration: none;
                            font-weight: 600;
                            margin-top: 5px;
                            display: inline-block;
                        }}
        
                        .support-link:hover {{
                            text-decoration: underline;
                        }}
        
                        .copyright {{
                            margin-top: 15px;
                            color: #a0aec0;
                            font-size: 12px;
                        }}
        
                        /* Mobile responsive */
                        @media only screen and (max-width: 480px) {{
                            .container {{
                                margin: 10px;
                                border-radius: 16px;
                            }}
            
                            .header {{
                                padding: 30px 20px;
                            }}
            
                            .brand-name {{
                                font-size: 24px;
                            }}
            
                            .header h1 {{
                                font-size: 22px;
                            }}
            
                            .content {{
                                padding: 20px;
                            }}
            
                            .greeting {{
                                font-size: 20px;
                            }}
            
                            .button {{
                                padding: 14px 30px;
                                font-size: 14px;
                                display: block;
                                width: 100%;
                                box-sizing: border-box;
                            }}
            
                            .expiry-badge {{
                                display: block;
                                text-align: center;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <!-- Header -->
                        <div class=""header"">
                            <span class=""brand-name"">GYMUNITY</span>
                            <h1>Reset Your Password</h1>
                        </div>
        
                        <!-- Main Content -->
                        <div class=""content"">
                            <div class=""reset-card"">
                                <h2 class=""greeting"">Hello {userName},</h2>
                                <p class=""message"">We received a request to reset your Gymunity account password. Don't worry, we've got you covered!</p>
                
                                <div style=""text-align: center;"">
                                    <span class=""expiry-badge"">⏳ Link expires in 10 minutes</span>
                                </div>
                
                                <div class=""button-container"">
                                    <a href=""{resetLink}"" class=""button"">🔓 RESET PASSWORD NOW</a>
                                </div>
                
                                <div class=""warning-box"">
                                    <p><strong>⚠️ Didn't request this?</strong> If you didn't request a password reset, please ignore this email or contact support if you're concerned.</p>
                                </div>
                            </div>
            
                            <hr class=""divider"">
            
                            <!-- Footer with proper alignment -->
                            <div class=""footer-content"">
                                <p>
                                    Sent with <span class=""heart"">❤️</span> from the Gymunity Team
                                </p>
                                <a href=""mailto:support@gymunity.com"" class=""support-link"">
                                    📧 Contact Support
                                </a>
                                <div class=""copyright"">
                                    &copy; 2026 Gymunity. All rights reserved.
                                </div>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        /// <summary>
        /// Reset Password Confirmation Email Template
        /// </summary>
        public string GetResetPasswordConfirmationEmail(string userName)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""utf-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        /* Reset styles */
                        body, html {{
                            margin: 0;
                            padding: 0;
                            font-family: 'Nunito', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background: linear-gradient(135deg, #00b09b 0%, #96c93d 100%);
                            -webkit-font-smoothing: antialiased;
                            -moz-osx-font-smoothing: grayscale;
                        }}
        
                        /* Main container */
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border-radius: 20px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,176,155,0.3);
                        }}
        
                        /* Header section */
                        .header {{
                            background: linear-gradient(135deg, #00b09b 0%, #96c93d 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
        
                        .brand-name {{
                            font-size: 28px;
                            font-weight: 800;
                            letter-spacing: 3px;
                            display: block;
                            margin-bottom: 10px;
                            text-transform: uppercase;
                            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
                        }}
        
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: 500;
                        }}
        
                        /* Content section */
                        .content {{
                            padding: 30px;
                            background: #f8fafc;
                        }}
        
                        /* Success card */
                        .success-card {{
                            background: white;
                            border-radius: 16px;
                            padding: 25px;
                            margin: 20px 0;
                            border-left: 5px solid #00b09b;
                            box-shadow: 0 5px 20px rgba(0,176,155,0.1);
                            text-align: center;
                        }}
        
                        .success-heading {{
                            color: #00b09b;
                            font-size: 28px;
                            margin: 0 0 15px 0;
                        }}
        
                        .greeting {{
                            font-size: 20px;
                            color: #333;
                            margin: 0 0 10px 0;
                        }}
        
                        .message {{
                            font-size: 16px;
                            color: #4a5568;
                            margin-bottom: 20px;
                            line-height: 1.6;
                        }}
        
                        /* Shield card */
                        .shield-card {{
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            border-radius: 12px;
                            padding: 20px;
                            margin: 20px 0;
                            color: white;
                        }}
        
                        .shield-card p {{
                            margin: 0;
                            font-size: 16px;
                            line-height: 1.5;
                        }}
        
                        /* Button */
                        .button-container {{
                            text-align: center;
                            margin: 30px 0 20px;
                        }}
        
                        .button {{
                            display: inline-block;
                            padding: 16px 40px;
                            background: linear-gradient(135deg, #00b09b, #96c93d);
                            color: white !important;
                            text-decoration: none;
                            border-radius: 50px;
                            font-weight: 600;
                            font-size: 16px;
                            box-shadow: 0 10px 20px rgba(0,176,155,0.3);
                            transition: transform 0.3s;
                        }}
        
                        .button:hover {{
                            transform: translateY(-2px);
                        }}
        
                        /* Divider */
                        .divider {{
                            border: none;
                            border-top: 2px dashed #cbd5e0;
                            margin: 30px 0;
                        }}
        
                        /* Footer section */
                        .footer-content {{
                            text-align: center;
                            color: #718096;
                            font-size: 13px;
                            line-height: 1.6;
                            padding: 0 20px 20px;
                        }}
        
                        .heart {{
                            color: #f56565;
                            font-size: 16px;
                        }}
        
                        .support-link {{
                            color: #00b09b;
                            text-decoration: none;
                            font-weight: 600;
                            margin-top: 5px;
                            display: inline-block;
                        }}
        
                        .support-link:hover {{
                            text-decoration: underline;
                        }}
        
                        .copyright {{
                            margin-top: 15px;
                            color: #a0aec0;
                            font-size: 12px;
                        }}
        
                        /* Mobile responsive */
                        @media only screen and (max-width: 480px) {{
                            .container {{
                                margin: 10px;
                                border-radius: 16px;
                            }}
            
                            .header {{
                                padding: 30px 20px;
                            }}
            
                            .brand-name {{
                                font-size: 24px;
                            }}
            
                            .header h1 {{
                                font-size: 22px;
                            }}
            
                            .content {{
                                padding: 20px;
                            }}
            
                            .success-heading {{
                                font-size: 24px;
                            }}
            
                            .greeting {{
                                font-size: 18px;
                            }}
            
                            .button {{
                                padding: 14px 30px;
                                font-size: 14px;
                                display: block;
                                width: 100%;
                                box-sizing: border-box;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <!-- Header -->
                        <div class=""header"">
                            <span class=""brand-name"">GYMUNITY</span>
                            <h1>Password Updated!</h1>
                        </div>
        
                        <!-- Main Content -->
                        <div class=""content"">
                            <div class=""success-card"">
                                <h2 class=""success-heading"">Success! 🎉</h2>
                                <h3 class=""greeting"">Hello {userName},</h3>
                
                                <div class=""shield-card"">
                                    <p>🛡️ Your password has been successfully reset. You can now log in with your new password.</p>
                                </div>
                
                                <div class=""button-container"">
                                    <a href=""https://gymunity.com/login"" class=""button"">🔐 GO TO LOGIN</a>
                                </div>
                
                                <p style=""color: #718096; font-size: 14px; margin-top: 20px;"">
                                    If you didn't make this change, please contact us immediately.
                                </p>
                            </div>
            
                            <hr class=""divider"">
            
                            <!-- Footer with proper alignment -->
                            <div class=""footer-content"">
                                <p>
                                    Sent with <span class=""heart"">❤️</span> from the Gymunity Team
                                </p>
                                <a href=""mailto:support@gymunity.com"" class=""support-link"">
                                    📧 Contact Support
                                </a>
                                <div class=""copyright"">
                                    &copy; 2026 Gymunity. All rights reserved.
                                </div>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        /// <summary>
        /// Change Password Confirmation Email Template
        /// </summary>
        public string GetChangePasswordConfirmationEmail(string userName, string changeDate, string changeTime, string device)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""utf-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        /* Reset styles */
                        body, html {{
                            margin: 0;
                            padding: 0;
                            font-family: 'Raleway', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            -webkit-font-smoothing: antialiased;
                            -moz-osx-font-smoothing: grayscale;
                        }}
        
                        /* Main container */
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border-radius: 20px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(102,126,234,0.3);
                        }}
        
                        /* Header section */
                        .header {{
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
        
                        .brand-name {{
                            font-size: 28px;
                            font-weight: 800;
                            letter-spacing: 3px;
                            display: block;
                            margin-bottom: 10px;
                            text-transform: uppercase;
                            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
                        }}
        
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: 500;
                        }}
        
                        /* Content section */
                        .content {{
                            padding: 30px;
                            background: #f8fafc;
                        }}
        
                        /* Security badge */
                        .security-badge {{
                            background: linear-gradient(135deg, #ff6b6b, #feca57);
                            color: white;
                            padding: 8px 20px;
                            border-radius: 50px;
                            display: inline-block;
                            font-weight: 600;
                            font-size: 14px;
                            margin-bottom: 20px;
                        }}
        
                        /* Info card */
                        .info-card {{
                            background: white;
                            border-radius: 16px;
                            padding: 25px;
                            margin: 20px 0;
                            border-left: 5px solid #667eea;
                            box-shadow: 0 5px 20px rgba(102,126,234,0.1);
                        }}
        
                        .greeting {{
                            color: #667eea;
                            font-size: 22px;
                            margin: 0 0 15px 0;
                        }}
        
                        .message {{
                            font-size: 16px;
                            color: #333;
                            margin-bottom: 20px;
                            line-height: 1.5;
                        }}
        
                        /* Activity box */
                        .activity-box {{
                            background: #f5f7fa;
                            border-radius: 12px;
                            padding: 20px;
                            margin: 20px 0;
                        }}
        
                        .activity-grid {{
                            display: flex;
                            justify-content: space-between;
                            gap: 10px;
                        }}
        
                        .activity-item {{
                            flex: 1;
                            text-align: center;
                            padding: 10px;
                            background: white;
                            border-radius: 8px;
                        }}
        
                        .activity-label {{
                            color: #718096;
                            font-size: 12px;
                            margin-bottom: 5px;
                        }}
        
                        .activity-value {{
                            color: #667eea;
                            font-weight: 700;
                            font-size: 14px;
                        }}
        
                        /* Button group */
                        .button-group {{
                            display: flex;
                            gap: 15px;
                            margin: 30px 0;
                        }}
        
                        .btn-primary {{
                            flex: 1;
                            padding: 12px 20px;
                            background: linear-gradient(135deg, #667eea, #764ba2);
                            color: white !important;
                            text-decoration: none;
                            border-radius: 50px;
                            font-weight: 600;
                            font-size: 14px;
                            text-align: center;
                            transition: transform 0.3s;
                        }}
        
                        .btn-secondary {{
                            flex: 1;
                            padding: 12px 20px;
                            background: white;
                            color: #667eea !important;
                            text-decoration: none;
                            border-radius: 50px;
                            font-weight: 600;
                            font-size: 14px;
                            text-align: center;
                            border: 2px solid #667eea;
                            transition: transform 0.3s;
                        }}
        
                        .btn-primary:hover, .btn-secondary:hover {{
                            transform: translateY(-2px);
                        }}
        
                        /* Warning box */
                        .warning-box {{
                            background: #fff3cd;
                            border-left: 5px solid #ffc107;
                            border-radius: 12px;
                            padding: 20px;
                            margin: 20px 0;
                            color: #856404;
                        }}
        
                        .warning-box p {{
                            margin: 0;
                            font-size: 14px;
                        }}
        
                        .warning-box a {{
                            color: #856404;
                            font-weight: 700;
                        }}
        
                        /* Divider */
                        .divider {{
                            border: none;
                            border-top: 2px dashed #cbd5e0;
                            margin: 30px 0;
                        }}
        
                        /* Footer section */
                        .footer-content {{
                            text-align: center;
                            color: #718096;
                            font-size: 13px;
                            line-height: 1.6;
                            padding: 0 20px 20px;
                        }}
        
                        .heart {{
                            color: #f56565;
                            font-size: 16px;
                        }}
        
                        .support-link {{
                            color: #667eea;
                            text-decoration: none;
                            font-weight: 600;
                            margin-top: 5px;
                            display: inline-block;
                        }}
        
                        .support-link:hover {{
                            text-decoration: underline;
                        }}
        
                        .copyright {{
                            margin-top: 15px;
                            color: #a0aec0;
                            font-size: 12px;
                        }}
        
                        /* Mobile responsive */
                        @media only screen and (max-width: 480px) {{
                            .container {{
                                margin: 10px;
                                border-radius: 16px;
                            }}
            
                            .header {{
                                padding: 30px 20px;
                            }}
            
                            .brand-name {{
                                font-size: 24px;
                            }}
            
                            .header h1 {{
                                font-size: 22px;
                            }}
            
                            .content {{
                                padding: 20px;
                            }}
            
                            .greeting {{
                                font-size: 18px;
                            }}
            
                            .activity-grid {{
                                flex-direction: column;
                            }}
            
                            .button-group {{
                                flex-direction: column;
                            }}
            
                            .btn-primary, .btn-secondary {{
                                width: 100%;
                            }}
            
                            .security-badge {{
                                display: block;
                                text-align: center;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <!-- Header -->
                        <div class=""header"">
                            <span class=""brand-name"">GYMUNITY</span>
                            <h1>Password Changed</h1>
                        </div>
        
                        <!-- Main Content -->
                        <div class=""content"">
                            <div style=""text-align: center;"">
                                <span class=""security-badge"">🔒 SECURITY UPDATE</span>
                            </div>
            
                            <div class=""info-card"">
                                <h2 class=""greeting"">Hi {userName},</h2>
                                <p class=""message"">We're confirming that your Gymunity account password was successfully changed on:</p>
                
                                <div class=""activity-box"">
                                    <div class=""activity-grid"">
                                        <div class=""activity-item"">
                                            <div class=""activity-label"">Date</div>
                                            <div class=""activity-value"">{changeDate}</div>
                                        </div>
                                        <div class=""activity-item"">
                                            <div class=""activity-label"">Time</div>
                                            <div class=""activity-value"">{changeTime}</div>
                                        </div>
                                        <div class=""activity-item"">
                                            <div class=""activity-label"">Device</div>
                                            <div class=""activity-value"">{device}</div>
                                        </div>
                                    </div>
                                </div>
                
                                <div class=""button-group"">
                                    <a href=""https://gymunity.com/account"" class=""btn-primary"">👤 MY ACCOUNT</a>
                                    <a href=""https://gymunity.com/activity"" class=""btn-secondary"">📊 VIEW ACTIVITY</a>
                                </div>
                
                                <div class=""warning-box"">
                                    <p>
                                        <strong>⚠️ Didn't make this change?</strong> 
                                        If you didn't change your password, 
                                        <a href=""https://gymunity.com/support"">contact support immediately</a>.
                                    </p>
                                </div>
                            </div>
            
                            <hr class=""divider"">
            
                            <!-- Footer with proper alignment -->
                            <div class=""footer-content"">
                                <p>
                                    Sent with <span class=""heart"">❤️</span> from the Gymunity Team
                                </p>
                                <a href=""mailto:support@gymunity.com"" class=""support-link"">
                                    📧 Contact Support
                                </a>
                                <div class=""copyright"">
                                    &copy; 2026 Gymunity. All rights reserved.
                                </div>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        /// <summary>
        /// Generic email template with custom content
        /// </summary>
        public string GetCustomEmailTemplate(string title, string message, string buttonText = null, string buttonLink = null)
        {
            string buttonHtml = "";
            if (!string.IsNullOrEmpty(buttonText) && !string.IsNullOrEmpty(buttonLink))
            {
                buttonHtml = $@"
            <div class=""button-container"">
                <a href=""{buttonLink}"" class=""button"">{buttonText}</a>
            </div>";
            }

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""utf-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        /* Reset styles */
                        body, html {{
                            margin: 0;
                            padding: 0;
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            -webkit-font-smoothing: antialiased;
                            -moz-osx-font-smoothing: grayscale;
                        }}
        
                        /* Main container */
                        .container {{
                            max-width: 600px;
                            margin: 20px auto;
                            background-color: #ffffff;
                            border-radius: 20px;
                            overflow: hidden;
                            box-shadow: 0 20px 40px rgba(0,0,0,0.15);
                        }}
        
                        /* Header section */
                        .header {{
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            padding: 40px 30px;
                            text-align: center;
                            color: white;
                        }}
        
                        .brand-name {{
                            font-size: 28px;
                            font-weight: 800;
                            letter-spacing: 3px;
                            display: block;
                            margin-bottom: 10px;
                            text-transform: uppercase;
                            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
                        }}
        
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: 500;
                        }}
        
                        /* Content section */
                        .content {{
                            padding: 30px;
                            background: #f8fafc;
                        }}
        
                        /* Message card */
                        .message-card {{
                            background: white;
                            border-radius: 16px;
                            padding: 25px;
                            margin: 20px 0;
                            border-left: 5px solid #667eea;
                            box-shadow: 0 5px 20px rgba(102,126,234,0.1);
                        }}
        
                        .message-content {{
                            font-size: 16px;
                            color: #333;
                            line-height: 1.6;
                        }}
        
                        .message-content p {{
                            margin: 0 0 15px 0;
                        }}
        
                        .message-content p:last-child {{
                            margin-bottom: 0;
                        }}
        
                        /* Button */
                        .button-container {{
                            text-align: center;
                            margin: 30px 0 20px;
                        }}
        
                        .button {{
                            display: inline-block;
                            padding: 16px 40px;
                            background: linear-gradient(135deg, #667eea, #764ba2);
                            color: white !important;
                            text-decoration: none;
                            border-radius: 50px;
                            font-weight: 600;
                            font-size: 16px;
                            box-shadow: 0 10px 20px rgba(102,126,234,0.3);
                            transition: transform 0.3s;
                        }}
        
                        .button:hover {{
                            transform: translateY(-2px);
                        }}
        
                        /* Divider */
                        .divider {{
                            border: none;
                            border-top: 2px dashed #cbd5e0;
                            margin: 30px 0;
                        }}
        
                        /* Footer section */
                        .footer-content {{
                            text-align: center;
                            color: #718096;
                            font-size: 13px;
                            line-height: 1.6;
                            padding: 0 20px 20px;
                        }}
        
                        .heart {{
                            color: #f56565;
                            font-size: 16px;
                        }}
        
                        .support-link {{
                            color: #667eea;
                            text-decoration: none;
                            font-weight: 600;
                            margin-top: 5px;
                            display: inline-block;
                        }}
        
                        .support-link:hover {{
                            text-decoration: underline;
                        }}
        
                        .copyright {{
                            margin-top: 15px;
                            color: #a0aec0;
                            font-size: 12px;
                        }}
        
                        /* Mobile responsive */
                        @media only screen and (max-width: 480px) {{
                            .container {{
                                margin: 10px;
                                border-radius: 16px;
                            }}
            
                            .header {{
                                padding: 30px 20px;
                            }}
            
                            .brand-name {{
                                font-size: 24px;
                            }}
            
                            .header h1 {{
                                font-size: 22px;
                            }}
            
                            .content {{
                                padding: 20px;
                            }}
            
                            .button {{
                                padding: 14px 30px;
                                font-size: 14px;
                                display: block;
                                width: 100%;
                                box-sizing: border-box;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <!-- Header -->
                        <div class=""header"">
                            <span class=""brand-name"">GYMUNITY</span>
                            <h1>{title}</h1>
                        </div>
        
                        <!-- Main Content -->
                        <div class=""content"">
                            <div class=""message-card"">
                                <div class=""message-content"">
                                    {message}
                                </div>
                            </div>
            
                            {buttonHtml}
            
                            <hr class=""divider"">
            
                            <!-- Footer with proper alignment -->
                            <div class=""footer-content"">
                                <p>
                                    Sent with <span class=""heart"">❤️</span> from the Gymunity Team
                                </p>
                                <a href=""mailto:support@gymunity.com"" class=""support-link"">
                                    📧 Contact Support
                                </a>
                                <div class=""copyright"">
                                    &copy; 2026 Gymunity. All rights reserved.
                                </div>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}