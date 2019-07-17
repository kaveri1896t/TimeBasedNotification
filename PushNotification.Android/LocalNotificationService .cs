using System;
using PushNotification.Interface;
using Android.App;
using Android.Content;
using PushNotification.Model;
using PushNotification.Droid;
using Java.Lang;
using Android.Support.V4.App;
using System.Xml.Serialization;
using System.IO;
using Android.Media;
using Android.OS;
using Android.Content.Res;

[assembly: Xamarin.Forms.Dependency(typeof(LocalNotificationService))]

namespace PushNotification.Droid
{
    class LocalNotificationService : ILocalNotificationService
    {
        int _notificationIconId { get; set; }
        readonly DateTime _jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal string _randomNumber;
        public LocalNotification localNotification = new LocalNotification();
        public void LocalNotification(string title, string body, int id, DateTime notifyTime)
        {

            //long repeateDay = 1000 * 60 * 60 * 24;       
            long repeateForMinute = 60000; // In milliseconds      
            long totalMilliSeconds = (long)(notifyTime.ToUniversalTime() - _jan1st1970).TotalMilliseconds;
            if (totalMilliSeconds < JavaSystem.CurrentTimeMillis())
            {
                totalMilliSeconds = totalMilliSeconds + repeateForMinute;
            }

            var intent = CreateIntent(id);
           
            localNotification.Title = title;
            localNotification.Body = body;
            localNotification.Id = id;
            localNotification.NotifyTime = notifyTime;
           // this._notificationIconId = 1;
            if (_notificationIconId != 0)
            {
                localNotification.IconId = _notificationIconId;
            }
            else
            {
                localNotification.IconId = Resource.Drawable.addReminder;
            }

            var serializedNotification = SerializeNotification(localNotification);
            intent.PutExtra(ScheduledAlarmHandler.LocalNotificationKey, serializedNotification);

            Random generator = new Random();
            _randomNumber = generator.Next(100000, 999999).ToString("D6");

            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, Convert.ToInt32(_randomNumber), intent, PendingIntentFlags.OneShot);
            var alarmManager = GetAlarmManager();
            alarmManager.SetRepeating(AlarmType.RtcWakeup, totalMilliSeconds, repeateForMinute, pendingIntent);
        }

        public void Cancel(int id)
        {

            var intent = CreateIntent(id);
            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, Convert.ToInt32(_randomNumber), intent, PendingIntentFlags.OneShot);
            var alarmManager = GetAlarmManager();
            alarmManager.Cancel(pendingIntent);
            var notificationManager = NotificationManagerCompat.From(Application.Context);
            notificationManager.CancelAll();
            notificationManager.Cancel(id);
        }

        public static Intent GetLauncherActivity()
        {

            var packageName = Application.Context.PackageName;
            return Application.Context.PackageManager.GetLaunchIntentForPackage(packageName);
        }


        private Intent CreateIntent(int id)
        {

            return new Intent(Application.Context, typeof(ScheduledAlarmHandler))
                .SetAction("LocalNotifierIntent" + id);
        }

        private AlarmManager GetAlarmManager()
        {

            var alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            return alarmManager;
        }

        private string SerializeNotification(LocalNotification notification)
        {

            var xmlSerializer = new XmlSerializer(notification.GetType());

            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, notification);
                return stringWriter.ToString();
            }
        }


    }

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {

        public const string LocalNotificationKey = "LocalNotification";

        public override void OnReceive(Context context, Intent intent)
        {
           var intent1 = new Intent(context, typeof(MainActivity));
            intent1.AddFlags(ActivityFlags.ClearTop);
            var extra = intent.GetStringExtra(LocalNotificationKey);
            var notification = DeserializeNotification(extra);
           string CHANNEL_ID = "my_channel_id_01";
            Random random = new Random();
            int randomNumber = random.Next(9999 - 1000) + 1000;
            var pendingIntent = PendingIntent.GetActivity(Application.Context, randomNumber, intent1, PendingIntentFlags.OneShot);
            // var resultIntent = LocalNotificationService.GetLauncherActivity();

            NotificationCompat.Builder builder = new NotificationCompat.Builder(Application.Context, CHANNEL_ID)
          .SetContentTitle(notification.Title)
          .SetContentText(notification.Body)
          .SetContentIntent(pendingIntent)
          .SetSmallIcon(Resource.Drawable.icon);

            // Build the notification:
            Notification notification1 = builder.Build();

           

           /* resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            var stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(Application.Context);
            stackBuilder.AddNextIntent(resultIntent);*/

            // Get the notification manager:
            NotificationManager notificationManager =
               context.GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification1);

        }


        /* public override void OnReceive(Context context, Intent intent)
         {
            /* Random random = new Random();
             int randomNumber = random.Next(9999 - 1000) + 1000;
             var pendingIntent = PendingIntent.GetBroadcast(Application.Context, Convert.ToInt32(randomNumber), intent, PendingIntentFlags.OneShot);

             var extra = intent.GetStringExtra(LocalNotificationKey);
             var notification = DeserializeNotification(extra);
             string CHANNEL_ID = "my_channel_id_01";
             //Generating notification       
             /*  var builder = new NotificationCompat.Builder(Application.Context, NOTIFICATION_CHANNEL_ID);
                   .SetContentTitle(notification.Title)
                   .SetContentText(notification.Body)
                   .SetSmallIcon(notification.IconId)
                   .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                   .SetAutoCancel(true);
            // var cont = Application.Context;
             NotificationCompat.Builder builder = new NotificationCompat.Builder(Application.Context, CHANNEL_ID)
                             .SetSmallIcon(Resource.Drawable.addReminder)
                             .SetContentTitle(notification.Title)
                             .SetContentText(notification.Body)
                             .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                             .SetAutoCancel(true)
                             .SetContentIntent(pendingIntent)
                             .SetPriority(NotificationCompat.PriorityDefault);

             var resultIntent = LocalNotificationService.GetLauncherActivity();

             resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
             var stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(Application.Context);
             stackBuilder.AddNextIntent(resultIntent);


             Notification notify = builder.Build();
             //var resultPendingIntent = stackBuilder.GetPendingIntent(randomNumber, (int)PendingIntentFlags.OneShot);
           // builder.SetContentIntent(pendingIntent);
             // Sending notification       
            var notificationManager = NotificationManagerCompat.From(Application.Context);
            //NotificationManager notificationManager= context.GetSystemService(Context.NotificationService) as NotificationManager;
             notificationManager.Notify(randomNumber, notify);

              var extra = intent.GetStringExtra(LocalNotificationKey);    
             var notification = DeserializeNotification(extra);
             //Generating notification    

             var resultIntent = LocalNotificationService.GetLauncherActivity();
             resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
             var stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(Application.Context);
             stackBuilder.AddNextIntent(resultIntent);

             Random random = new Random();
             int randomNumber = random.Next(9999 - 1000) + 1000;

             var resultPendingIntent =
                 stackBuilder.GetPendingIntent(randomNumber, (int)PendingIntentFlags.OneShot);

             var builder = new NotificationCompat.Builder(Application.Context)    
                 .SetContentTitle(notification.Title)    
                 .SetContentText(notification.Body)    
                 .SetSmallIcon(notification.IconId)    
                 .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                 .SetContentIntent(resultPendingIntent)
                 .SetAutoCancel(true);    



             // Sending notification    
             var notificationManager = NotificationManagerCompat.From(Application.Context);    
             notificationManager.Notify(randomNumber, builder.Build());    
         }*/

        private LocalNotification DeserializeNotification(string notificationString)
        {

            var xmlSerializer = new XmlSerializer(typeof(LocalNotification));
            using (var stringReader = new StringReader(notificationString))
            {
                var notification = (LocalNotification)xmlSerializer.Deserialize(stringReader);
                return notification;
            }
        }
    }
}