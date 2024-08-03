--drop proc GetDailyReceptionsNum
--go
create proc GetDailyReceptionsNum
@start date,
@finish date
as
declare @day date
set @day = @start
create table #days(day date);
while @day !> @finish
begin
	insert #days select @day
	set @day = dateadd(day, 1, @day)
end
select d.day as 'Day', count(id) as 'Cnt' from 
#days d
  left join 
receptions r
  on 
  DATEPART(year, r.StartDateTime)=DATEPART(year, d.day)
  and DATEPART(DAYOFYEAR, r.StartDateTime)=DATEPART(DAYOFYEAR, d.day)

group by d.day
order by Day
go