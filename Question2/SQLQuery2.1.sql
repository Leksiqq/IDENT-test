select 
d.id as 'id_doc',
per.lastname as 'doc_lastname', 
p.id as 'pat_id',
per1.lastname as 'pat_lastname' 
from
doctors d
inner join 
patients p
	on d.id=(select top 1 id_doctors from receptions where id_patients=p.id order by StartDateTime desc)
inner join
persons per
	on d.id=per.id
inner join
persons per1	
	on p.id=per1.id