with counts as (
select count(*) as 'Cnt', CONVERT(DATE, StartDateTime) AS 'Day' from receptions
group by CONVERT(DATE, StartDateTime)
)
select Cnt, Day from counts 
where 
	Day between cast('2015-01-01' as date)
	and cast('2015-12-31' as date)
order by Day
