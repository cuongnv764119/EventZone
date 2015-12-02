using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EventZone.Models;

namespace EventZone.Helpers
{
      public class GroupByActorNotfication : IEqualityComparer<NotificationChange>
        {

            public bool Equals(NotificationChange x, NotificationChange y)
            {
                return x.ActorID==y.ActorID;
            }

            public int GetHashCode(NotificationChange obj)
            {
                return obj.ActorID.GetHashCode();
            }
        }
      public class GroupByActorIdAndEventID : IEqualityComparer<NotificationChange> {
          public bool Equals(NotificationChange x, NotificationChange y)
          {
              return x.ActorID == y.ActorID && x.EventID == y.EventID;
          }

          public int GetHashCode(NotificationChange obj)
          {
              return obj.ActorID.GetHashCode()^obj.EventID.GetHashCode();
          }
      }
      public class GroupByEventIDNotification : IEqualityComparer<NotificationChange> {
          public bool Equals(NotificationChange x, NotificationChange y)
          {
              return x.EventID == y.EventID;
          }

          public int GetHashCode(NotificationChange obj)
          {
              return obj.EventID.GetHashCode();
          }
      }
}