// Copyright (c) 2010 Martin Knafve / hMailServer.com.  
// http://www.hmailserver.com

#pragma once

namespace HM
{
   class SQLCommand;

   class DALRecordset  
   {
   public:
      DALRecordset();
      virtual ~DALRecordset();

      bool Open(boost::shared_ptr<DALConnection> pConn, const SQLCommand &command);

      virtual DALConnection::ExecutionResult TryOpen(boost::shared_ptr<DALConnection> pConn, const SQLCommand &command, String &sErrorMessage) = 0;

      virtual long RecordCount() const = 0;
      virtual bool MoveNext() = 0;
      virtual bool IsEOF() const = 0;

      

      virtual String GetStringValue(const AnsiString &FieldName) const= 0;
      virtual long GetLongValue(const AnsiString &FieldName) const = 0;
      virtual __int64 GetInt64Value(const AnsiString &FieldName) const =0 ;
      virtual double GetDoubleValue(const AnsiString &FieldName) const = 0;

      virtual bool GetIsNull(const AnsiString &FieldName) const = 0;
      virtual vector<AnsiString> GetColumnNames() const = 0;



   protected:
      void _ReportEOFError(const AnsiString &FieldName) const;

   private:

   };
}
