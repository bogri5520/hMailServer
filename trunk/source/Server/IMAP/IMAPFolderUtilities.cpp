// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#include "stdafx.h"
#include "IMAPFolderUtilities.h"

#include "../IMAP/IMAPConfiguration.h"
#include "../Common/BO/IMAPFolder.h"
#include "../Common/BO/IMAPFolders.h"

namespace HM
{
   IMAPFolderUtilities::IMAPFolderUtilities()
   {
      
   }

   IMAPFolderUtilities::~IMAPFolderUtilities()
   {

   }

   bool 
   IMAPFolderUtilities::IsPublicFolder(const std::vector<String> &vecFolderPath)
   {
      if (vecFolderPath.size() == 0)
         return false;

      String sPublicFolderName = Configuration::Instance()->GetIMAPConfiguration()->GetIMAPPublicFolderName();
      if (sPublicFolderName.CompareNoCase(vecFolderPath[0]) == 0)
         return true;
      else
         return false;
   }
   
   shared_ptr<IMAPFolder> 
   IMAPFolderUtilities::GetTopMostExistingFolder(shared_ptr<IMAPFolders> pContainer, const std::vector<String> &vecFolderPath)
   {
      vector<String> tempFolderPath = vecFolderPath;
      shared_ptr<IMAPFolder> pTempFolder = pContainer->GetFolderByFullPath(tempFolderPath);

      while (!pTempFolder && tempFolderPath.size() > 0)
      {
         tempFolderPath.erase(tempFolderPath.end() - 1);
         
         pTempFolder = pContainer->GetFolderByFullPath(tempFolderPath);   
      }

      return pTempFolder;
   }


}