// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#include "IMAPCommand.h"

namespace HM
{
   class IMAPCommandCLOSE : public IMAPCommand
   {
      virtual IMAPResult ExecuteCommand(boost::shared_ptr<HM::IMAPConnection> pConnection, boost::shared_ptr<IMAPCommandArgument> pArgument);
   };

}

