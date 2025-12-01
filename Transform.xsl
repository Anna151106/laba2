<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<html>
			<body>
				<h2>Успішність студентів</h2>
				<table border="1">
					<tr bgcolor="#9acd32">
						<th>Ім'я</th>
						<th>Факультет</th>
						<th>Предмет</th>
						<th>Оцінка</th>
					</tr>
					<xsl:for-each select="University/Student/Subject">
						<tr>
							<td>
								<xsl:value-of select="../@Name"/>
							</td>
							<td>
								<xsl:value-of select="../@Faculty"/>
							</td>
							<td>
								<xsl:value-of select="@Name"/>
							</td>
							<td>
								<xsl:value-of select="@Grade"/>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>