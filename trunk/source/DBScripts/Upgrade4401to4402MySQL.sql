ALTER TABLE hm_rule_actions ADD COLUMN actionrouteid int not null DEFAULT 0

update hm_dbversion set value = 4402

