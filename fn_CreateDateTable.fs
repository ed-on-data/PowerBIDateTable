let
    CreateDateTable = (Prefix,StartDate as date,EndDate as date) =>
let
    pPrefix = if Prefix="" then "" else Text.Clean(Prefix)&" ",
    pStartDate= if StartDate="" then #date(2019,01,01) else StartDate,
    pEndDate = if EndDate="" then #date(Date.Year(DateTime.LocalNow()),12,31) else EndDate,

    //Create lists of month and day names for use later on
    MonthList = {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"},
    DayList = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"},
    
	//Find the number of days between the end date and the start date
    NumberOfDates = Duration.Days(pEndDate-pStartDate),
    
	//Generate a continuous list of dates from the start date to the end date
    DateList = List.Dates(StartDate, NumberOfDates, #duration(1, 0, 0, 0)),
    
	//Turn this list into a table
    TableFromList = Table.FromList(DateList, Splitter.SplitByNothing(), {"Date"}
                     , null, ExtraValues.Error),
    
	//Caste the single column in the table to type date
    ChangedType = Table.TransformColumnTypes(TableFromList,{{"Date", type date}}),
    
	//Add custom columns for day of month, month number, year
    DayOfMonth = Table.AddColumn(ChangedType, "DayOfMonth", each Date.Day([Date])),
    MonthNumber = Table.AddColumn(DayOfMonth, "MonthNumberOfYear", each Date.Month([Date])),
    Year = Table.AddColumn(MonthNumber, "Year", each Date.Year([Date])),
    DayOfWeekNumber = Table.AddColumn(Year, "DayOfWeekNumber", each Date.DayOfWeek([Date])+1),
    
	//Since Power Query doesn't have functions to return day or month names, 
    //use the lists created earlier for this
    MonthName = Table.AddColumn(DayOfWeekNumber, "MonthName", each MonthList{[MonthNumberOfYear]-1}),
    DayName = Table.AddColumn(MonthName, "DayName", each Date.ToText([Date],"ddd")),
    
    IsToday = Table.AddColumn(DayName, "IsToday", each Date.IsInCurrentDay([Date])),
    YearsAgo = Table.AddColumn(IsToday, "Years Ago", each Number.From([Year] - Date.Year(DateTime.LocalNow()))),
    MonthsAgo = Table.AddColumn(#"YearsAgo", "Months Ago", each ([MonthNumberOfYear]+(12*[Years Ago])) - Date.Month(DateTime.LocalNow())),
    Month = Table.AddColumn(#"MonthsAgo", "Month", each Text.Start([MonthName],3)&" '"&Text.End(Number.ToText([Year]),2)),

    FirstDayOfWeek = Table.AddColumn(Month, "FirstDayOfWeek", each Date.AddDays([Date], (([DayOfWeekNumber])*-1))),
    LastDayOfWeek = Table.AddColumn(FirstDayOfWeek, "LastDayOfWeek", each Date.AddDays([Date],6-[DayOfWeekNumber])),
    Week = Table.AddColumn(LastDayOfWeek, "Week", each Date.ToText([FirstDayOfWeek],"MMM-dd")&" to "&Date.ToText([LastDayOfWeek],"MMM-dd")),
    WeeksAgo = Table.AddColumn(Week, "Weeks Ago", each Number.From([FirstDayOfWeek] - Date.From(Date.AddDays(DateTime.LocalNow(),(Date.DayOfWeek(DateTime.LocalNow(),Day.Monday)*-1)-1)))/7),
    Quarter = Table.AddColumn(WeeksAgo,"Quarter", each Number.From(Date.QuarterOfYear([Date]))) ,

    RenameColumns = if pPrefix="" 
    then Table.RenameColumns(Quarter, Table.ToRows(Table.AddColumn(Table.FromList(Table.ColumnNames(Quarter)), "New Column Name", each Text.Insert ( [Column1] ,0, pPrefix ))))
    else Quarter 

in
    RenameColumns
in
    CreateDateTable
