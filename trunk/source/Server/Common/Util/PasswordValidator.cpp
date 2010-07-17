// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#include "StdAfx.h"
#include ".\passwordvalidator.h"

#include "../Application/ObjectCache.h"
#include "../Application/DefaultDomain.h"
#include "../Cache/CacheContainer.h"
#include "../BO/Account.h"
#include "../BO/Domain.h"
#include "../BO/DomainAliases.h"
#include "../Util/SSPIValidation.h"
#include "../Util/Crypt.h"
#include "../Persistence/PersistentDomain.h"
#include "../Persistence/PersistentAccount.h"

namespace HM
{
   PasswordValidator::PasswordValidator(void)
   {
   }

   PasswordValidator::~PasswordValidator(void)
   {
   }

   shared_ptr<const Account>
   PasswordValidator::ValidatePassword(const String &sUsername, const String &sPassword)
   {
      shared_ptr<Account> pEmpty;

      // Apply domain name aliases to this domain name.
      shared_ptr<DomainAliases> pDA = ObjectCache::Instance()->GetDomainAliases();
      String sAccountAddress = pDA->ApplyAliasesOnAddress(sUsername);

      // Apply default domain
      sAccountAddress = DefaultDomain::ApplyDefaultDomain(sAccountAddress);

      shared_ptr<const Account> pAccount = CacheContainer::Instance()->GetAccount(sAccountAddress);
      
      if (!pAccount)
         return pEmpty;

      if (!pAccount->GetActive())
         return pEmpty;

      // Check that the domain is active as well.
      
      String sDomain = StringParser::ExtractDomain(sAccountAddress);
      shared_ptr<const Domain> pDomain = CacheContainer::Instance()->GetDomain(sDomain);

      if (!pDomain)
         return pEmpty;

      if (!pDomain->GetIsActive())
         return pEmpty;

      if (!ValidatePassword(pAccount, sPassword))
         return pEmpty;



      return pAccount;
   }

   bool 
   PasswordValidator::ValidatePassword(shared_ptr<const Account> pAccount, const String &sPassword)
   {
      if (sPassword.GetLength() == 0)
      {
         // Empty passwords are not permitted.
         return false;
      }

      // Check if this is an active directory account.
      if (pAccount->GetIsAD())
      {
         String sADDomain = pAccount->GetADDomain();
         String sADUsername = pAccount->GetADUsername();

         bool bUserOK = SSPIValidation::ValidateUser(sADDomain, sADUsername, sPassword);

         if(bUserOK)
            return true;
         else
            return false;
      }

      Crypt::EncryptionType iPasswordEncryption = (Crypt::EncryptionType) pAccount->GetPasswordEncryption();

      String sComparePassword = pAccount->GetPassword();

      if (iPasswordEncryption == 0)
      {
         // Do plain text comparision
         sComparePassword.MakeLower();

         if (sPassword.CompareNoCase(sComparePassword) != 0)
            return false;
      }
      else if (iPasswordEncryption == Crypt::ETMD5 ||
               iPasswordEncryption == Crypt::ETSHA256)
      {
         // Compare hashs
         bool result = Crypt::Instance()->Validate(sPassword, sComparePassword, iPasswordEncryption);

         if (!result)
            return false;
      }
      else if (iPasswordEncryption == Crypt::ETBlowFish)
      {
         String decrypted = Crypt::Instance()->DeCrypt(sComparePassword, iPasswordEncryption);

         if (sPassword.CompareNoCase(decrypted) != 0)
            return false;
      }
      else
         return false;

      return true;
   }

}