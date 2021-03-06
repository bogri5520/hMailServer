// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#pragma once
#include "../hMailServer/resource.h"       // main symbols

#include "../hMailServer/hMailServer.h"

namespace HM
{
   class Accounts;
   class PerisstentAccount;
   class COMAuthentication;
}

class ATL_NO_VTABLE InterfaceAccounts : 
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<InterfaceAccounts, &CLSID_Accounts>,
	public IDispatchImpl<IInterfaceAccounts, &IID_IInterfaceAccounts, &LIBID_hMailServer, /*wMajor =*/ 1, /*wMinor =*/ 0>,
   public HM::COMAuthenticator
{
public:

   InterfaceAccounts()
   {
#ifdef _DEBUG
      InterlockedIncrement(&counter);

#endif
      m_iDomainID = 0;
   }

   ~InterfaceAccounts()
   {
#ifdef _DEBUG
      InterlockedDecrement(&counter);
#endif
   }

   // void SetAuthentication(boost::shared_ptr<HM::COMAuthentication> pAuthentication);

DECLARE_REGISTRY_RESOURCEID(IDR_INTERFACEACCOUNTS)


BEGIN_COM_MAP(InterfaceAccounts)
	COM_INTERFACE_ENTRY(IInterfaceAccounts)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()


	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}
	
	void FinalRelease() 
	{
	}

   STDMETHOD(DeleteByDBID)(/*[in]*/ long DBID);
   STDMETHOD(get_ItemByAddress)(/*[in]*/ BSTR Address, /*[out, retval]*/ IInterfaceAccount* *pVal);
   STDMETHOD(get_ItemByDBID)(/*[in]*/ long DBID, /*[out, retval]*/ IInterfaceAccount **pVal);
   STDMETHOD(Refresh)();
   STDMETHOD(get_Item)(/*[in]*/ long Index, /*[out, retval]*/ IInterfaceAccount **pVal);
   STDMETHOD(Delete)(/*[in]*/ long Index);
   STDMETHOD(Add)(/*[out, retval]*/ IInterfaceAccount** pVal);
   STDMETHOD(get_Count)(/*[out, retval]*/ long *pVal);

   void Attach(boost::shared_ptr<HM::Accounts> pAccounts);
   void SetDomain(__int64 Domain) { m_iDomainID = Domain; }

private:

   boost::shared_ptr<HM::Accounts> m_pAccounts;
   //boost::shared_ptr<HM::COMAuthentication> m_pAuthentication;

   __int64 m_iDomainID;

#ifdef _DEBUG
   static long counter;
#endif


};

OBJECT_ENTRY_AUTO(__uuidof(Accounts), InterfaceAccounts)
