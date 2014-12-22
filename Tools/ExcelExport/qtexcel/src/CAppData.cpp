﻿#include "CAppData.hxx"
#include "ExcelTblSort.hxx"
#include "CTask.hxx"
#include <cstring>
#include <QtGui/QtGui>
#include <QtWidgets>
#include "Tools.hxx"

template<> CAppData* Singleton<CAppData>::msSingleton = 0;

CAppData::CAppData()
{

}

CAppData::~CAppData()
{

}

CTask& CAppData::getTask()
{
	return m_task;
}

void CAppData::initData()
{
	m_excelTbl = new ExcelTblSort();
	m_task.readXML();
}

ExcelTbl* CAppData::getExcelTbl()
{
	return m_excelTbl;
}

// start Multi
void CAppData::startMultiPack()
{
	std::vector<CSolution>::iterator ite;
	std::vector<CPackage*>::iterator itePack;
	for (ite = m_task.getSolutionLst().begin(); ite != m_task.getSolutionLst().end(); ++ite)
	{
		if(strcmp(ite->getName().c_str(), m_xmlSolution.c_str()))
		{
			break;
		}
	}

	if(ite !=  m_task.getSolutionLst().end())
	{
		// package table
		for (itePack = ite->getPackLst().begin(); itePack != ite->getPackLst().begin(); ++itePack)
		{
			m_excelTbl->setXmlPath((*itePack)->getXml().c_str());
			m_excelTbl->setOutputPath((*itePack)->getOutput().c_str());
			m_excelTbl->convExcel2Tbl();
		}

		// execute external program
		if(ite->getCmd().length())
		{
			QProcess::startDetached(ite->getCmd().c_str(), QStringList());
		}
	}
}

// start single
void CAppData::startSinglePack()
{
	m_excelTbl->convExcel2Tbl();
}

void CAppData::setXml(string outpath, string xmlpath, string xmlsolution)
{
	m_outPath = outpath;
	m_xmlFile = xmlpath;
	m_xmlSolution = xmlsolution;

	m_excelTbl->setXmlPath(m_xmlFile.c_str());
	m_excelTbl->setOutputPath(m_outPath.c_str());
}

bool CAppData::isSetSolution()
{
	return m_xmlSolution.length();
}

void CAppData::initThread(QThread* pthread)
{
	m_pthread = pthread;
}

void CAppData::startThread()
{
	m_pthread->start();
}

void CAppData::initCombo(QComboBox *comboBoxSolution)
{
	QString tmp;
	std::vector<CSolution>::iterator ite;
	for (ite = m_task.getSolutionLst().begin(); ite != m_task.getSolutionLst().end(); ++ite)
	{
		//comboBoxSolution->addItem(QString::fromLocal8Bit((ite->getName().c_str())));
		tmp = Tools::getSingletonPtr()->GBKChar2UNICODEStr(ite->getName().c_str());
		comboBoxSolution->addItem(tmp);
	}
}