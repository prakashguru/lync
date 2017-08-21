using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Exchange.WebServices.Data;
using System.Net;

namespace AudioVideoConversation
{
    public partial class MeetingFinder : UserControl
    {
        public event EventHandler<StartMeetingEventArgs> StartMeeting;
        private const int MeetingRangeHours = 2;
        private ExchangeService _exchangeService;

        public MeetingFinder()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Meeting Selected.
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select a meeting....");
                return;
            }

            StartMeeting(this, new StartMeetingEventArgs() { Appointment = ((CustomAppointment)comboBox1.SelectedItem).Appointment });
        }

        public bool StartLoad()
        {
            Console.WriteLine("Connecting to Exchange to retrive appointment details....");

            try
            {
                _exchangeService = new ExchangeService(ExchangeVersion.Exchange2013);

                if (!GetExternalUrl())
                    return false;

                bool wasSignedIn = TryWebCredentials();

                if (!wasSignedIn)
                    wasSignedIn = TryAutoDiscover();

                if (wasSignedIn)
                    Console.WriteLine("Exchange Connection OK");

                CalendarView calendarView = new CalendarView(DateTime.Now.Subtract(TimeSpan.FromHours(MeetingRangeHours)), DateTime.Now.Add(TimeSpan.FromHours(MeetingRangeHours)), 1000);

                var openAppointments = GetAppointmentsWithOnlineMeeting(calendarView).Select(d => new CustomAppointment(d)).ToList();

                Console.WriteLine("Available Appointments : " + openAppointments.Count);

                if (this.InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        this.comboBox1.DataSource = openAppointments;
                    }));
                }
                else {
                    this.comboBox1.DataSource = openAppointments;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exchange connection failed. Unable to retrive appointment details.");
            }

            return true;
        }

        private bool GetExternalUrl()
        {
            if (!string.IsNullOrEmpty(Program.ExchangeControlPanelUrl))
            {
                Uri ecpUri = new Uri(Program.ExchangeControlPanelUrl);
                _exchangeService.Url = new Uri($"https://{ecpUri.Host}/EWS/Exchange.asmx");
                return true;
            }
            return false;
        }

        private bool TryAutoDiscover()
        {
            try
            {
                _exchangeService.AutodiscoverUrl(Program.UserName);
                return true;
            }
            catch (AutodiscoverLocalException)
            {
                Console.WriteLine("Autodiscover Exception, connection not yet available.");
                return false;
            }
        }

        private bool TryWebCredentials()
        {
            try
            {
                _exchangeService.Credentials = new NetworkCredential(Program.UserName, Program.Password);
                return GetExternalUrl();
            }
            catch (Exception)
            {
                Console.WriteLine("Web credentials failed");
                return false;
            }
        }

        private class CustomAppointment
        {
            public CustomAppointment(Appointment app)
            {
                this.Appointment = app;
            }

            public Appointment Appointment { get; private set; }

            public override string ToString()
            {
                return string.Format(" {2} - [{0} - {1}]", Appointment.Start.ToString("MMM dd, hh:mm"), Appointment.End.ToString("MMM dd, hh:mm"), Appointment.Subject);
            }
        }

        private IEnumerable<Appointment> GetAppointmentsWithOnlineMeeting(CalendarView calendarView)
        {
            ExtendedPropertyDefinition openedConferenceId = new ExtendedPropertyDefinition(
                DefaultExtendedPropertySet.PublicStrings, "UCOpenedConferenceID", MapiPropertyType.String);
            calendarView.PropertySet = new PropertySet(BasePropertySet.FirstClassProperties)
            {
                openedConferenceId
            };

            FindItemsResults<Appointment> items = _exchangeService.FindAppointments(new FolderId(WellKnownFolderName.Calendar), calendarView);

            return items.Where(appointment => !string.IsNullOrEmpty(appointment.JoinOnlineMeetingUrl));
        }
    }

   
}
