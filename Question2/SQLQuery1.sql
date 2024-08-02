with days as (
select dateadd(day, value, cast('2015-01-01' as date)) as 'day' from generate_series(0, 364, 1)
)
select count(*) as 'Cnt', days.day as 'Day' from receptions inner join days 
on CONVERT(DATE, Receptions.StartDateTime)=days.day group by days.day
order by Day
