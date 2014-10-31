using System;
using System.Collections.Generic;
using System.Text;

namespace MqttLib
{
  public class CompleteArgs
  {

    #region Member Variables

      private ulong _messageID;

    #endregion

    #region Constructors

      public CompleteArgs(ulong messageID)
    {
      _messageID = messageID;
    }

    #endregion

    #region Properties

    /// <summary>
    /// ID of the message
    /// </summary>
      public ulong MessageID
      {
          get
          {
              return _messageID;
          }
      }

    #endregion
  }
}
