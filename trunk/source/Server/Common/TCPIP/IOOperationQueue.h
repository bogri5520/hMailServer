// Copyright (c) 2005 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com
// Created 2005-07-21

#pragma once

#include "IOOperation.h"

namespace HM
{

   class IOOperationQueue
   {
   public:
      IOOperationQueue();
      ~IOOperationQueue(void);

      void Push(boost::shared_ptr<IOOperation> operation);
      boost::shared_ptr<IOOperation> Front();
      void Pop(IOOperation::OperationType type);

      bool ContainsQueuedSendOperation();

   private:

      CriticalSection _criticalSection;

      std::deque<boost::shared_ptr<IOOperation> > _queueOperations;
      
      std::vector<boost::shared_ptr<IOOperation > > _ongoingOperations;
   };
}