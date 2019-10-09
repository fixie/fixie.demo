Database migration scripts in this folder must use the following naming convention.

Scripts affecting all environments:

    YYYY-MM-DD-HHMM.DescriptionOfChange.sql

Scripts affecting one environment:

    YYYY-MM-DD-HHMM.DescriptionOfChange.ENV.DEV.sql

Scripts affecting multiple environments:

    YYYY-MM-DD-HHMM.DescriptionOfChange.ENV.DEV.STAGING.sql

The YYYY-MM-DD-HHMM timestamp ensures sorting by creation time. HH should be from 00 to 24.