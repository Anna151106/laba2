<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/">
		<html>
			<body>
				<h2>Успішність студентів (Зведені дані з деталізацією)</h2>
				<table border="1">
					<tr bgcolor="#9acd32">
						<th>Ім'я</th>
						<th>Факультет</th>
						<th>Предмети та оцінки</th>
						<th>Середній бал</th>
					</tr>

					<xsl:for-each select="University/Student">
						<tr>
							<td>
								<xsl:value-of select="@Name"/>
							</td>
							<td>
								<xsl:value-of select="@Faculty"/>
							</td>

							<td>
								<xsl:for-each select="Subject">
									<xsl:value-of select="concat(@Name, ' (', @Grade, ')')"/>

									<xsl:if test="position() != last()">
										<xsl:text>, </xsl:text>
									</xsl:if>
								</xsl:for-each>
							</td>

							<td>
								<xsl:value-of select="format-number(sum(Subject/@Grade) div count(Subject), '0.00')"/>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>