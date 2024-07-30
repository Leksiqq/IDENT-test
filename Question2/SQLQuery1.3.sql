with days as (
select distinct CONVERT(DATE, StartDateTime) AS 'Day' from receptions 
where 
	StartDateTime between 
cast('2015-01-01 00:00:00.000' as datetime)
and DATEADD(ms, -1, cast('2016-01-01 00:00:00.000' as datetime))
)
select count(*) as 'Cnt', days.day as 'Day' from receptions inner join days 
on CONVERT(DATE, Receptions.StartDateTime)=days.day group by days.day
order by Day
