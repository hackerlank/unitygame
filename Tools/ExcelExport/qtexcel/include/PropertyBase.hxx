#ifndef _PROPERTYBASE_H
#define _PROPERTYBASE_H

#include "ByteBuffer.hxx"
#include "Platform.hxx"

BEGINNAMESPACE(NSExcelExport)

class PropertyBase
{
protected:
	// 这个是通用序列化
	virtual void srz2BU(ByteBuffer& byteBuffer) = 0;

public:
	// 序列化服务器
	virtual void srz2BUServer(ByteBuffer& byteBuffer) = 0;
	// 序列化桌面客户端
	virtual void srz2BUDesktop(ByteBuffer& byteBuffer) = 0;
	// 序列化 web
	virtual void srz2BUWeb(ByteBuffer& byteBuffer) = 0;
	// 序列化移动设备
	virtual void srz2BUMobile(ByteBuffer& byteBuffer) = 0;
};

ENDNAMESPACE(NSExcelExport)

#endif	// _PROPERTYBASE_H  