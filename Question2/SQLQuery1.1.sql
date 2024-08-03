with days as (
  select dateadd(day, value, cast('20150101' as date)) as 'day' from   generate_series(0, 364, 1)
)
select days.day as 'Day', count(id) as 'Cnt' from 
days 
  left join 
receptions
  on 
  DATEPART(year, Receptions.StartDateTime)=DATEPART(year, days.day)
  and DATEPART(DAYOFYEAR, Receptions.StartDateTime)=DATEPART(DAYOFYEAR, days.day)

group by days.day
order by Day
