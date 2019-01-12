using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Сводное описание для DeviceStateMode
/// </summary>
public class DeviceStateMode
{
    public DeviceStateMode()
    {
        //
        // TODO: добавьте логику конструктора
        //
    }

    /// <summary>
    /// Режимы ККТ
    /// </summary>
    public enum StateMode
    {
        /// <summary>
        /// Выбор
        /// </summary>
        Selection = 0x00,
        /// <summary>
        /// Регистрация: Ожидание команды
        /// </summary>
        Registration_WaitingForCommand = 0x01,
        /// <summary>
        /// Регистрация: Ввод пароля
        /// </summary>
        Registration_EnteringPassword = 0x11,
        /// <summary>
        /// Регистрация: Ожидание ввода секции
        /// </summary>
        Registration_WaitingForSection = 0x21,
        /// <summary>
        /// Регистрация: Ожидание сторно по штрихкоду
        /// </summary>
        Registration_WaitingForSTORNObyBarCode = 0x31,
        /// <summary>
        /// Регистрация: Прием платежей
        /// </summary>
        Registration_PaymentInProgress = 0x41,
        /// <summary>
        /// Регистрация: Ожидание печати отложенного документа
        /// </summary>
        Registration_WaitForDelayedDocPrint = 0x51,
        /// <summary>
        /// Регистрация: Печать отложенного документа
        /// </summary>
        Registration_DelayedDocumentPrintInProgress = 0x61,
        /// <summary>
        /// Регистрация: Формирование документа
        /// </summary>
        Registration_DocumentFormInProgress = 0x71,
        /// <summary>
        /// Отчеты о состоянии счетчиков: Ожидание команды
        /// </summary>
        CounterStateReports_WaitingForCommand = 0x02,
        /// <summary>
        /// Отчеты о состоянии счетчиков: Ввод пароля
        /// </summary>
        CounterStateReports_EnteringPassword = 0x12,
        /// <summary>
        /// Отчеты о состоянии счетчиков: Формирование отчета о состоянии счетчиков ККТ
        /// </summary>
        CounterStateReports_CountersStateReportInProgress = 0x22,
        /// <summary>
        /// Отчеты о состоянии счетчиков: Формирование служебного отчета
        /// </summary>
        CounterStateReports_ServiceReportInProgress = 0x32,
        /// <summary>
        /// Отчет о закрытии смены: Ожидание команды
        /// </summary>
        StageReports_WaitingForCommand = 0x03,
        /// <summary>
        /// Отчет о закрытии смены: Ввод пароля
        /// </summary>
        StageReports_EnteringPassword = 0x13,
        /// <summary>
        /// Отчет о закрытии смены: Формирование отчета о закрытии смены
        /// </summary>
        StageReports_StageCloseReportInProgress = 0x23,
        /// <summary>
        /// Отчет о закрытии смены: Подтверждение гашения счетчиков
        /// </summary>
        StageReports_CountersClearConfirmation = 0x33,
        /// <summary>
        /// Отчет о закрытии смены: Ввод даты с клавиатуры
        /// </summary>
        StageReports_ManualDateEntryInProgress = 0x43,
        /// <summary>
        /// Отчет о закрытии смены: Ожидание подтверждения общего гашения счетчиков
        /// </summary>
        StageReports_WaitingForCountersClearConfirmation = 0x53,
        /// <summary>
        /// Отчет о закрытии смены: Идет общее гашение
        /// </summary>
        StageReports_TotalClearingInProgress = 0x63,
        /// <summary>
        /// Программирование: Ожидание команды
        /// </summary>
        Program_WaitingForCommand = 0x04,
        /// <summary>
        /// Программирование: Ввод пароля
        /// </summary>
        Program_EnteringPassword = 0x14,
        /// <summary>
        /// Ввод ЗН: Ожидание команды
        /// </summary>
        EnterMfgNumber_WaitingForCommand = 0x05,
        /// <summary>
        /// Ввод ЗН: Ввод пароля
        /// </summary>
        EnterMfgNumber_EnteringPassword = 0x15,
        /// <summary>
        /// Ввод ЗН: Описание отсутствует
        /// </summary>
        EnterMfgNumber_EMPTY = 0x25,
        /// <summary>
        /// Ввод ЗН: Ввод данных
        /// </summary>
        EnterMfgNumber_EnteringDataInProgress = 0x35,
        /// <summary>
        /// Ввод ЗН: Подтверждение ввода
        /// </summary>
        EnterMfgNumber_ConfirmEnteredData = 0x45,
        /// <summary>
        /// Доступ к ФН: Ожидание команды
        /// </summary>
        FNAccess_WaitingForCommand = 0x06,
        /// <summary>
        /// Доступ к ФН: Формирование отчета
        /// </summary>
        FNAccess_ReportInProgress = 0x26,
        /// <summary>
        /// Дополнительный: Идет обнуление таблиц и гашение операционных регистров
        /// </summary>
        Additional_RegistersClearingInProgress = 0x07,
        /// <summary>
        /// Дополнительный: Выполняется тестовый прогон
        /// </summary>
        Additional_TestPassInProgress = 0x27,
        /// <summary>
        /// Дополнительный: Режим ввода времени с клавиатуры
        /// </summary>
        Additional_ManualTimeEntryInProgress = 0x37,
        /// <summary>
        /// Дополнительный: Режим тестов (для технологической ККТ)
        /// </summary>
        Additional_TestMode = 0x47,
        /// <summary>
        /// Дополнительный: Ввод даты после сбоя часов
        /// </summary>
        Additional_EnteringDateAfterClockFailed = 0x57,
        /// <summary>
        /// Дополнительный: Ввод времени после сбоя часов
        /// </summary>
        Additional_EnteringTimeAfterClockFailed = 0x67,
        /// <summary>
        /// Дополнительный: Начальная инициализация ККТ
        /// </summary>
        Additional_Initialization = 0x77,
        /// <summary>
        /// Дополнительный: Ожидание подтверждения обнуления таблиц
        /// </summary>
        Additional_WaitingForTablesClear = 0x87,
        /// <summary>
        /// Дополнительный: Разные накопители памяти
        /// </summary>
        Additional_DifferentStorageMedia = 0x97,
        /// <summary>
        /// Дополнительный: ККТ не инициализирована
        /// </summary>
        Additional_DeviceNotInitialized = 0xA7,
        /// <summary>
        /// Дополнительный: ККТ заблокирована при вводе даты, меньшей даты последней записи ФН
        /// </summary>
        Additional_DeviceLockedDueDateTampering = 0xB7,
        /// <summary>
        /// Дополнительный: Подтверждение ввода даты
        /// </summary>
        Additional_ConfirmEnteredData = 0xC7,
        /// <summary>
        /// Дополнительный: Оповещение о переводе часов на летнее/зимнее время
        /// </summary>
        Additional_DayLightSavingNotification = 713,
        /// <summary>
        /// Дополнительный: Блокировка при ошибке ФН
        /// </summary>
        Additional_DeviceLockedDueFNError = 0xD7,
    }
}