-wordInfo (get info for 1 word
@word = the word to search
@pos = the pos to search):

SELECT        word.lemma AS Word, sense.rank AS Sense, synset.synsetid, categorydef.pos, synset.definition, categorydef.SubType, xwnwsd.wsd
FROM            xwnwsd RIGHT OUTER JOIN
                         synset ON xwnwsd.synsetid = synset.synsetid LEFT OUTER JOIN
                         categorydef ON synset.categoryid = categorydef.categoryid RIGHT OUTER JOIN
                         sense RIGHT OUTER JOIN
                         word ON sense.wordid = word.wordid ON synset.synsetid = sense.synsetid
WHERE        (word.lemma = @word)
ORDER BY categorydef.pos, Sense


SELECT        word.lemma AS Word, sense.rank AS Sense, synset.synsetid, categorydef.pos, synset.definition, categorydef.SubType, xwnwsd.wsd
FROM            xwnwsd RIGHT OUTER JOIN
                         synset ON xwnwsd.synsetid = synset.synsetid LEFT OUTER JOIN
                         categorydef ON synset.categoryid = categorydef.categoryid RIGHT OUTER JOIN
                         sense RIGHT OUTER JOIN
                         word ON sense.wordid = word.wordid ON synset.synsetid = sense.synsetid
WHERE        (word.lemma = @word AND synset.synsetid =@id)


SELECT        word.lemma AS Word, sense.rank AS Sense, synset.synsetid, categorydef.pos, synset.definition, categorydef.SubType, xwnwsd.wsd
FROM            xwnwsd RIGHT OUTER JOIN
                         synset ON xwnwsd.synsetid = synset.synsetid LEFT OUTER JOIN
                         categorydef ON synset.categoryid = categorydef.categoryid RIGHT OUTER JOIN
                         sense RIGHT OUTER JOIN
                         word ON sense.wordid = word.wordid ON synset.synsetid = sense.synsetid
WHERE        (word.lemma = @word) AND (categorydef.pos = @pos)
ORDER BY categorydef.pos, Sense

---------------------------------------------
get synonyms 

SELECT        sense.synsetid, word.lemma AS word
FROM            word INNER JOIN
                         sense ON word.wordid = sense.wordid
WHERE        (sense.synsetid = @synset)

----------------------------------------------
compound words:

SELECT        word.lemma AS Word, sense.rank AS Sense, synset.synsetid, categorydef.pos, synset.definition, categorydef.SubType, xwnwsd.wsd, xwnwsd.text
FROM            xwnwsd RIGHT OUTER JOIN
                         synset ON xwnwsd.synsetid = synset.synsetid LEFT OUTER JOIN
                         categorydef ON synset.categoryid = categorydef.categoryid RIGHT OUTER JOIN
                         sense RIGHT OUTER JOIN
                         word ON sense.wordid = word.wordid ON synset.synsetid = sense.synsetid
WHERE        (word.lemma LIKE '% ' + @word + ' %') OR
                         (word.lemma LIKE '% ' + @word) OR
                         (word.lemma LIKE @word + ' %')
ORDER BY categorydef.pos, Sense


-------------------------------------------------
related words:
@linkid = the relationship to search for
@synsetid = the wordgroup to search relationships for

SELECT        semlinkref.linkid, linkdef.name, semlinkref.synset2id AS synsetid
FROM            linkdef INNER JOIN
                         semlinkref ON linkdef.linkid = semlinkref.linkid
WHERE        (semlinkref.linkid = @linkid) AND (semlinkref.synset1id = @synsetid)


SELECT        lexlinkref.linkid, linkdef.name, lexlinkref.synset2id AS synsetid
FROM            lexlinkref INNER JOIN
                         linkdef ON lexlinkref.linkid = linkdef.linkid
WHERE        (lexlinkref.synset1id = @synsetid)
GROUP BY lexlinkref.linkid, linkdef.name, lexlinkref.synset2id
HAVING        (lexlinkref.linkid = @linkid)


SELECT        COUNT(*) AS Expr1
FROM            (SELECT        lexlinkref.linkid, linkdef.name, lexlinkref.synset2id AS synsetid
                          FROM            lexlinkref INNER JOIN
                                                    linkdef ON lexlinkref.linkid = linkdef.linkid
                          WHERE        (lexlinkref.synset1id = @synsetid)
                          GROUP BY lexlinkref.linkid, linkdef.name, lexlinkref.synset2id
                          HAVING         (lexlinkref.linkid = @linkid)) AS derivedtbl_1
						  
						  
SELECT        COUNT(*) AS Expr1
FROM            semlinkref
WHERE        (linkid = @linkid) AND (synset1id = @synsetid)


SELECT        semlinkref.linkid, linkdef.name, semlinkref.synset1id AS synsetid
FROM            linkdef INNER JOIN
                         semlinkref ON linkdef.linkid = semlinkref.linkid
WHERE        (semlinkref.linkid = @linkid) AND (semlinkref.synset2id = @synsetid)						  


SELECT        lexlinkref.linkid, linkdef.name, lexlinkref.synset1id AS synsetid
FROM            lexlinkref INNER JOIN
                         linkdef ON lexlinkref.linkid = linkdef.linkid
WHERE        (lexlinkref.linkid = @linkid) AND (lexlinkref.synset2id = @synsetid)


---------------------------------------------------
relationships

SELECT        linkid, name, recurses
FROM            linkdef

(checks if a relationship is recursive or not)

SELECT        recurses
FROM            linkdef
WHERE        (linkid = @id)