select W.SchID, W.SchName, S.SPrice
from WSchema W
join speciality spec on w.speccode = spec.scode  and w.kodoper = @kodoper
join Schpricecontractor S on (W.SchID = S.SchID and S.Kateg = 13 and S.PriceType = 0)
join PricesContractor P on (S.PrID = P.PrID and P.FDate <= 'today' and P.Filial = 5)
where W.StructID = 3 and  (W.DisDate is null or W.DisDate > 'today')
and (P.PrID is not null or w.iscaption = '1')
order by w.schid, p.fdate desc
rows 1