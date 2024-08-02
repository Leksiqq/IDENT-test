select
  d.id as 'id_doc',
  per.lastname as 'doc_lastname', 
  r1.id_patients as 'pat_id',
  per1.lastname as 'pat_lastname',
  r1.StartDateTime
from 
  doctors d
    inner join
  (select r.id_patients, r.id_doctors, r.StartDateTime from 
    (select id_patients, id_doctors, StartDateTime, row_number() over (partition by id_patients ORDER BY StartDateTime desc) as rownumber from Receptions) r
    where r.rownumber=1
  ) r1
    on d.id=r1.id_doctors
    inner join
  persons per
	on d.id=per.id
    inner join
  persons per1	
	on r1.id_patients=per1.id
