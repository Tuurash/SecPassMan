using SecPassMan.Data;
using SecPassMan.Models;
using SecPassMan.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using Wpf.Ui.Controls;

namespace SecPassMan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{

    bool IsVerified = false;
    WebSocket webSocket { get; set; }
    HttpListener listener = new HttpListener();


    public ObservableCollection<SiteCredential> SiteCredentialsObservable { get; set; }

    public string MasterUserName { get; set; } = string.Empty;
    public string MasterEmail { get; set; } = string.Empty;
    public string MasterPassword { get; set; } = string.Empty;
    public string MasterPassForSignup { get; set; } = string.Empty;
    public string OTP = string.Empty;
    public string MasterOTP { get; set; }

    public string _SiteUserName { get; set; } = string.Empty;
    public string _SiteUri { get; set; } = string.Empty;
    public string _SitePassword { get; set; } = string.Empty;



    private SpDbContext context;
    public Credential MasterCreds { get; set; }
    public SiteCredentialCollection GetSiteCredentialCollection { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        context = new SpDbContext();
        Task.Run(async () => await ManageViaWebSocketAsync());

        if(!string.IsNullOrEmpty(GObservables.GlobalMasterPass))
        {
            //Check if master pass is correct
            try
            {
                Task.Run(async () =>
                {
                    MasterCreds = context.GetMasterCredentials().Find(x => x.MasterPassword == GObservables.GlobalMasterPass);

                    if (MasterCreds != null)
                    {
                        IsVerified = true;
                        await Dispatcher.InvokeAsync(() => lblMasterUserName.Content = MasterCreds.Name.ToString());

                        await LoadSiteCreds();
                    }
                    else
                    {
                        IsVerified = false;

                        await Dispatcher.InvokeAsync(() =>
                        {
                            panelSignIn.Visibility=System.Windows.Visibility.Visible;
                            panelSignUp.Visibility=System.Windows.Visibility.Hidden;
                        });
                    }
                });

            }
            catch (Exception exc)
            {
                IsVerified = false;
            }
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                UnAuthorizedView.Visibility = System.Windows.Visibility.Visible;
                AuthorizedView.Visibility = System.Windows.Visibility.Hidden;
            });
        }
    }

    private async Task LoadSiteCreds()
    {
        await Task.Delay(5);
        List<SiteCredential> siteCredentials = new List<SiteCredential>();

        try
        {
            siteCredentials = context.GetSiteCredentials(MasterCreds);


            if (siteCredentials!=null && siteCredentials.Count > 0)
            {
                //SiteCredentialsObservable = new ObservableCollection<SiteCredential>(siteCredentials);
                await Dispatcher.InvokeAsync(() =>
                {
                    SiteCredentialsObservable = new ObservableCollection<SiteCredential>(siteCredentials);

                    listSiteCreds.ItemsSource = siteCredentials;
                });

                GetSiteCredentialCollection = new SiteCredentialCollection()
                {
                    type = "siteCredentialCollection",
                    SiteCredentials = siteCredentials
                };
            }
            else
            {
                listSiteCreds.Items.Clear();
            }

        }catch(Exception exc)
        {
            Console.WriteLine(exc);
        }
    }

    private void ButtonDltSite_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            Button button = (Button)sender;
            var siteId = int.Parse(button.CommandParameter.ToString());

            var sCred = context.GetSiteCredentials(MasterCreds).Where(sc => sc.Id == siteId).FirstOrDefault();

            context.DeleteSiteCredential(sCred);

            Task.Run(async () => await LoadSiteCreds());
        }
        catch { }
    }

    private void btnLogin_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            MasterCreds = context.GetMasterCredentials().Find(x => x.MasterPassword == MasterPassword);


            if (MasterCreds != null)
            {

                IsVerified = true;
                Dispatcher.Invoke(() =>
                {

                    lblMasterUserName.Content = MasterCreds.Name.ToString();
                    MasterUserName = MasterPassForSignup = string.Empty;

                    UnAuthorizedView.Visibility = System.Windows.Visibility.Hidden;
                    AuthorizedView.Visibility = System.Windows.Visibility.Visible;
                });

                await LoadSiteCreds();
            }
        });
    }

    private void toggleSigninSignup_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if(panelSignIn.Visibility==System.Windows.Visibility.Visible)
        {

            toggleSigninSignup.Content=@"Sign In";


            panelSignIn.Visibility = System.Windows.Visibility.Hidden;
            panelSignUp.Visibility = System.Windows.Visibility.Visible;
        }
        else
        {
            toggleSigninSignup.Content = @"Create New Master User";
            panelSignIn.Visibility = System.Windows.Visibility.Visible;
            panelSignUp.Visibility = System.Windows.Visibility.Hidden;
        }
    }

    private void btnSignUp_Click(object sender, System.Windows.RoutedEventArgs e)
    {

        try
        {
            Task.Run(async() =>
            {
                if (!string.IsNullOrEmpty(MasterUserName) && !string.IsNullOrEmpty(MasterPassForSignup) &&!string.IsNullOrEmpty(MasterEmail))
                {
                    if(GObservables.IsValidEmailAddress(MasterEmail))
                    {
                        if(!string.IsNullOrEmpty(MasterOTP) &&!string.IsNullOrEmpty(OTP) && OTP == MasterOTP)
                        {
                            Credential credential = new Credential()
                            {
                                Name = MasterUserName,
                                MasterPassword = MasterPassForSignup,
                            };

                            context.SaveMasterCredential(credential);

                            MasterCreds = context.GetMasterCredentials().Find(x => x.MasterPassword == credential.MasterPassword);


                            if (MasterCreds != null)
                            {
                                GObservables.GlobalMasterPass = credential.MasterPassword;

                                IsVerified = true;
                                Dispatcher.Invoke(() =>
                                {

                                    lblMasterUserName.Content = MasterCreds.Name.ToString();
                                    MasterUserName = MasterPassForSignup = string.Empty;

                                    UnAuthorizedView.Visibility = System.Windows.Visibility.Hidden;
                                    AuthorizedView.Visibility = System.Windows.Visibility.Visible;
                                });

                                await LoadSiteCreds();
                            }
                        }
                    }
                }
            });
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
        }


    }

    private void btnSignUpCredAdded(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            Task.Run(async () =>
            {
                if (!string.IsNullOrEmpty(_SiteUserName) && !string.IsNullOrEmpty(_SiteUri) && !string.IsNullOrEmpty(_SitePassword))
                {
                    SiteCredential scredential = new SiteCredential()
                    {
                        SiteUsername = _SiteUserName,
                        SitePassword = _SitePassword,
                        SiteUrl=_SiteUri,
                        CredentialId= MasterCreds.Id
                    };

                    context.SaveSiteCredential(scredential);
                   
                    if (MasterCreds != null)
                    {
                        IsVerified = true;
                        Dispatcher.Invoke(() =>
                        {

                            lblMasterUserName.Content = MasterCreds.Name.ToString();
                            MasterUserName = MasterPassForSignup = string.Empty;

                            UnAuthorizedView.Visibility = System.Windows.Visibility.Hidden;
                            AuthorizedView.Visibility = System.Windows.Visibility.Visible;
                        });

                        await LoadSiteCreds();
                    }
                }
            });
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
        }
    }


    private async Task ManageViaWebSocketAsync()
    {
        //new WebSocket listener    
        try
        {
            listener = new HttpListener();
            HttpApi.ClearUrlReservation(GObservables.WebSocketUri);
            listener.Prefixes.Add("http://localhost:8080/");

            // Start the listener
            listener.Start();

            // Wait for a new WebSocket connection
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                // Accept the WebSocket connection
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                webSocket = webSocketContext.WebSocket;

                while (true)
                {
                    try
                    {
                        await SendMessageAsync(webSocket, "{\"type\": \"pong\"}");
                        // Start listening for WebSocket messages
                        await ReceiveMessages(webSocket);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await ManageViaWebSocketAsync();
                    }
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }

        }
        catch(Exception exc)
        {
            await Task.Delay(1000);
            listener.Start();
        }
    }

    private async Task ReceiveMessages(WebSocket webSocket)
    {
        var buffer = new byte[1024];

        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                // Wait for a new message
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Handle the received message
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received message: " + message);

                    if (!string.IsNullOrEmpty(message))
                    {
                        //if message is ping
                        if (message.Contains("ping"))
                        {
                            if(GetSiteCredentialCollection!=null && GetSiteCredentialCollection.SiteCredentials.Count>0) 
                            {
                                string jsonSerialized = JsonSerializer.Serialize(GetSiteCredentialCollection);
                                await SendMessageAsync(webSocket, jsonSerialized);
                            }
                            else
                            {
                                await SendMessageAsync(webSocket, "{\"type\": \"pong\"}");
                            }
                        }

                        //Serialize
                        //Database Check
                        //Stringify
                        //Send data to websocket



                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private async Task SendMessageAsync(WebSocket webSocket, string message)
    {
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private void btnRecieveOTP_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Task.Run(() =>
        {
            if (!string.IsNullOrEmpty(MasterUserName) && !string.IsNullOrEmpty(MasterPassForSignup) && !string.IsNullOrEmpty(MasterEmail))
            {
                if (GObservables.IsValidEmailAddress(MasterEmail))
                {
                    string _randomStringOTP = GObservables.RandomString(6);
                    OTP = _randomStringOTP;
                    MailService mailService = new MailService($"smtp.gmail.com", 587, $"arifultv2021@gmail.com", $"ddkxcjwdblrdrtpp");
                    mailService.SendEmail(MasterEmail, $"OTP | SecPassMan", $"<body> <table style='display: flex; align-content: center;padding: 1em;'> <tr> <td> <div align='center'> <h2> Secure Password Manager</h1> </div></td></tr><tr> <td> <div align='center'> <p style='font-size: 16;'> Hello, OTP for your current account is, </p></br> <h3>" + _randomStringOTP + "</h3> </div></td></tr></table></body>");
                }
            }
        });
    }

    private void btnGeneratePassword_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        string _randomString = GObservables.RandomString(16);
        _SitePassword = _randomString;
        txtGpass.Text = _SitePassword;
    }

    private void UiWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {

    }

    private void btnLogout_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Task.Run(() =>
        {
            MasterCreds = null;
            GObservables.GlobalMasterPass = string.Empty;
            MasterUserName = MasterPassForSignup = MasterPassword = string.Empty;

            Dispatcher.Invoke(() =>
            {

                lblMasterUserName.Content = string.Empty;

                txtMasterPassword.Text = string.Empty;

                UnAuthorizedView.Visibility = System.Windows.Visibility.Visible;
                AuthorizedView.Visibility = System.Windows.Visibility.Hidden;

                panelSignIn.Visibility = System.Windows.Visibility.Visible;
                panelSignUp.Visibility = System.Windows.Visibility.Hidden;
            });
        });
    }


}


