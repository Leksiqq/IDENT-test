select
  d.id as 'id_doc',
  per.lastname as 'doc_lastname',
  p.id as 'pat_id',
  per1.lastname as 'pat_lastname',
  r.StartDateTime
from
  doctors d
    inner join
  receptions r
    on d.id=r.id_doctors
    inner join
  patients p
    on r.id=(select top 1 id from receptions where id_patients=p.id and StartDateTime=(
    	select max(StartDateTime) from receptions where id_patients=p.id
  	  )
    )
    inner join
  persons per
    on d.id=per.id
    inner join
  persons per1    
    on p.id=per1.id
